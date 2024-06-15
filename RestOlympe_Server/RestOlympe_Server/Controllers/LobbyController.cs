using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestOlympe_Server.Data;
using RestOlympe_Server.Hubs;
using RestOlympe_Server.Models.DTO;
using RestOlympe_Server.Models.Entities;
using RestOlympe_Server.Services;
using System.ComponentModel.DataAnnotations;

namespace RestOlympe_Server.Controllers
{
    [ApiController]
    public class LobbyController : ControllerBase
    {

        private readonly ILogger<LobbyController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<RestoHub> _hub;
        private readonly OsmApiService _osmApi;

        public LobbyController(
            ILogger<LobbyController> logger,
            ApplicationDbContext context,
            IHubContext<RestoHub> hub,
            OsmApiService osmApi)
        {
            _logger = logger;
            _context = context;
            _hub = hub;
            _osmApi = osmApi;
        }

        [HttpGet]
        [Route("/[controller]")]
        [ProducesResponseType(200)]
        public IActionResult GetAllLobbies()
        {
            var lobbies = _context.Lobbies.Include(l => l.Admin).ToArray();

            return new JsonResult(new
            {
                lobbies
            });
        }

        [HttpPost]
        [Route("/[controller]")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult AddLobby(
            [FromForm] [Required] Guid adminId,
            [FromForm] [Required] [MaxLength(32)] string lobbyName,
            [FromForm] double? longitude,
            [FromForm] double? latitude,
            [FromForm] uint? voteRadiusKm
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!((longitude.HasValue && latitude.HasValue && voteRadiusKm.HasValue)
                || (!longitude.HasValue && !latitude.HasValue && !voteRadiusKm.HasValue))
            )
                return BadRequest("The fields longitude, latitude and voteRadiusKm must either all be absent or all be present.");

            var admin = _context.Users
                .Include(u => u.LobbiesAsUser)
                .Include(u => u.LobbiesAsAdmin)
                .SingleOrDefault(u => u.UserId == adminId);

            if (admin == null)
                return NotFound("Admin user does not exist.");

            var newLobby = new LobbyModel()
            {
                AdminId = adminId,
                Longitude = longitude,
                Latitude = latitude,
                VoteRadiusKm = voteRadiusKm,
                Admin = admin,
                Name = lobbyName,
                IsClosed = false,
                LobbyId = Guid.NewGuid(),
                Users = [admin]
            };

            _context.Lobbies.Add(newLobby);
            _context.SaveChanges();

            return Created($"/lobby/{newLobby.LobbyId}", newLobby);
        }

        [HttpGet]
        [Route("/[controller]/{lobbyId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetLobby([Required] Guid lobbyId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lobby = _context.Lobbies.Include(l => l.Admin).SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound($"This lobby does not exist.");

            return new JsonResult(new
            {
                lobby
            });
        }

        [HttpDelete]
        [Route("/[controller]/{lobbyId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult DeleteLobby(
            [Required] Guid lobbyId,
            [FromForm] [Required] Guid adminId
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lobby = _context.Lobbies.SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound($"This lobby does not exist.");

            if (lobby.AdminId != adminId)
                return Forbid();

            _context.Lobbies.Remove(lobby);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpPost]
        [Route("/[controller]/{lobbyId}/join")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult JoinLobby(
            [Required] Guid lobbyId,
            [FromForm] [Required] Guid userId
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _context.Users
                .SingleOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound("The specified user does not exist.");

            var lobby = _context.Lobbies
                .Include(l => l.Users)
                .SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            if (lobby.IsClosed)
                return BadRequest("The specified lobby is closed and cannot take any more votes");

            if (lobby.Users.Select(u => u.UserId).Contains(userId))
                return BadRequest("The specified user already joined the lobby.");

            lobby.Users.Add(user);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpGet]
        [Route("/[controller]/{lobbyId}/vote")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetLobbyVotes([Required] Guid lobbyId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lobby = _context.Lobbies.Include(l => l.Votes).SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            return new JsonResult(new
            {
                lobby.Votes
            });
        }

        [HttpPost]
        [Route("/[controller]/{lobbyId}/vote")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Vote(
             [Required] Guid lobbyId,
             [FromForm] [Required] Guid userId,
             [FromForm] [Required] long osmId,
             [FromForm] [Required] int voteValue
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lobby = _context.Lobbies.Include(l => l.Users).SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            if (lobby.IsClosed)
                return BadRequest("The specified lobby is closed and cannot take any more votes");

            var user = _context.Users.Include(u => u.Votes).SingleOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound("The specified user does not exist.");

            if (!lobby.Users.Any(u => u.UserId == user.UserId))
                return Forbid();

            if (user.Votes.Any(v => v.LobbyId == lobbyId && v.OsmId == osmId))
                return Conflict();

            if (user.Votes.Where(v => v.LobbyId == lobbyId).Sum(v => Math.Abs(v.Value)) + Math.Abs(voteValue) > 100)
                return BadRequest("User cannot user more than 100 points per lobby.");

            var restaurant = await _osmApi.GetAsync(osmId.ToString());

            if (restaurant == null)
                return NotFound("The specified restaurant does not exist.");

            var newVote = new VoteModel()
            {
                User = user,
                UserId = user.UserId,
                Lobby = lobby,
                LobbyId = lobby.LobbyId,
                Value = voteValue,
                OsmId = osmId
            };

            _context.Votes.Add(newVote);
            _context.SaveChanges();

            return Created($"/lobby/{lobby.LobbyId}/user/{user.UserId}/vote/{newVote.OsmId}", newVote);
        }


        [HttpPost]
        [Route("/[controller]/{lobbyId}/close")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult CloseLobby(
            [Required] Guid lobbyId,
            [FromForm] [Required] Guid adminId
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lobby = _context.Lobbies.SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            if (lobby.AdminId != adminId)
                return Forbid();

            lobby.IsClosed = true;
            _context.SaveChanges();

            _hub.Clients.All.SendAsync("LobbyClosed", lobbyId);

            return NoContent();
        }

        [HttpGet]
        [Route("/[controller]/{lobbyId}/user")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetLobbyUsers([Required] Guid lobbyId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lobby = _context.Lobbies.Include(l => l.Users).SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            return new JsonResult(new
            {
                lobby.Users
            });
        }

        [HttpGet]
        [Route("/[controller]/{lobbyId}/user/{userId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetLobbyUsers(
            [Required] Guid lobbyId,
            [Required] Guid userId
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lobby = _context.Lobbies.Include(l => l.Users).SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            var user = lobby.Users.SingleOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound("The specified user was not found in the lobby.");

            return new JsonResult(user);
        }

        [HttpDelete]
        [Route("/[controller]/{lobbyId}/user/{userId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult LeaveLobby(
            [Required] Guid lobbyId,
            [Required] Guid userId
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lobby = _context.Lobbies.Include(l => l.Users).SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            var user = lobby.Users.SingleOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound("The specified user was not found in the lobby.");

            lobby.Users.Remove(user);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpGet]
        [Route("/[controller]/{lobbyId}/user/{userId}/vote")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetLobbyUserVotes(
            [Required] Guid lobbyId,
            [Required] Guid userId
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lobby = _context.Lobbies.Include(l => l.Users).ThenInclude(u => u.Votes).SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            var user = lobby.Users.SingleOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound("The specified user was not found in the lobby.");

            var votes = user.Votes.Where(v => v.LobbyId == lobby.LobbyId);

            return new JsonResult(new
            {
                votes
            });
        }

        [HttpGet]
        [Route("/[controller]/{lobbyId}/user/{userId}/vote/{osmId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetLobbyUserVote(
            [Required] Guid lobbyId,
            [Required] Guid userId,
            [Required] long osmId
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lobby = _context.Lobbies.Include(l => l.Users).ThenInclude(u => u.Votes).SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            var user = lobby.Users.SingleOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound("The specified user was not found in the lobby.");

            var vote = user.Votes.SingleOrDefault(v => v.LobbyId == lobby.LobbyId && v.OsmId == osmId);

            if (vote == null)
                return NotFound("The specified vote was not found for this user in this lobby.");


            return new JsonResult(vote);
        }

        [HttpPatch]
        [Route("/[controller]/{lobbyId}/user/{userId}/vote/{osmId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult ChangeLobbyUserVote(
            [Required] Guid lobbyId,
            [Required] Guid userId,
            [Required] long osmId,
            [FromForm] [Required] int newValue
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lobby = _context.Lobbies.Include(l => l.Users).ThenInclude(u => u.Votes).SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            var user = lobby.Users.SingleOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound("The specified user was not found in the lobby.");

            var lobbyVotes = user.Votes.Where(v => v.LobbyId == lobby.LobbyId);
            var vote = lobbyVotes.SingleOrDefault(v => v.OsmId == osmId);

            if (vote == null)
                return NotFound("The specified vote was not found for this user in this lobby.");

            if (lobbyVotes.Sum(v => Math.Abs(v.Value)) - Math.Abs(vote.Value) + Math.Abs(newValue) > 100)
                return BadRequest("User cannot user more than 100 points per lobby.");

            vote.Value = newValue;
            _context.SaveChanges();

            return new JsonResult(vote);
        }

        [HttpDelete]
        [Route("/[controller]/{lobbyId}/user/{userId}/vote/{osmId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult DeleteLobbyUserVote(
            [Required] Guid lobbyId,
            [Required] Guid userId,
            [Required] long osmId
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lobby = _context.Lobbies.Include(l => l.Users).ThenInclude(u => u.Votes).SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            var user = lobby.Users.SingleOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound("The specified user was not found in the lobby.");

            var lobbyVotes = user.Votes.Where(v => v.LobbyId == lobby.LobbyId);
            var vote = lobbyVotes.SingleOrDefault(v => v.OsmId == osmId);

            if (vote == null)
                return NotFound("The specified vote was not found for this user in this lobby.");

            user.Votes.Remove(vote);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpGet]
        [Route("/[controller]/{lobbyId}/result")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetLobbyResult(
            [Required] Guid lobbyId
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lobby = _context.Lobbies.Include(l => l.Votes).SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            if (!lobby.IsClosed)
                return BadRequest("The lobby is not closed yet.");

            var results = new List<OpenRestaurantInResultDTO>();

            var groupedVotes = lobby.Votes.GroupBy(v => v.OsmId);

            foreach (var group in groupedVotes)
            {
                var restaurant = await _osmApi.GetAsync(group.Key.ToString());
                if (restaurant == null)
                    continue;
                results.Add(new(group.Sum(v => v.Value), restaurant));
            }

            return new JsonResult(new
            {
                results = results.OrderByDescending(r => r.VoteCount)
            });
        }

        [HttpGet]
        [Route("/[controller]/{lobbyId}/restaurant")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetRestaurants(
            [Required] Guid lobbyId,
            uint page = 0
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lobby = _context.Lobbies.Include(l => l.Votes).SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            RestaurantListDTO? result;

            if (lobby.VoteRadiusKm.HasValue && lobby.Longitude.HasValue && lobby.Latitude.HasValue)
                result = await _osmApi.GetListAroundLocationAsync(new GeoPoint(lobby.Longitude.Value, lobby.Latitude.Value), lobby.VoteRadiusKm.Value, page);
            else
                result = await _osmApi.GetListAroundLocationAsync(null, null, page);

            return new JsonResult(result);
        }
    }
}
