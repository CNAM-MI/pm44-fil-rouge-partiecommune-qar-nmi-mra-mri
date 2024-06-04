using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestOlympe_Server.Services;
using RestOlympe_Server.Data;
using RestOlympe_Server.Hubs;
using RestOlympe_Server.Models.Entities;
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
        public IActionResult AddLobby(
            [Required] Guid adminId,
            [Required][MaxLength(32)] string lobbyName,
            double? longitude,
            double? latitude,
            float? voteRadiusKm
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

        [HttpPost]
        [Route("/[controller]/{lobbyId}/join")]
        public IActionResult JoinLobby(
            [Required] Guid lobbyId,
            [Required] Guid userId
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

            if (lobby.Users.Select(u => u.UserId).Contains(userId))
                return BadRequest("The specified user already joined the lobby.");

            lobby.Users.Add(user);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpPost]
        [Route("/[controller]/{lobbyId}/vote")]
        public async Task<IActionResult> Vote(
            Guid lobbyId,
            Guid userId,
            int osmId,
            int voteValue
        )
        {
            var lobby = _context.Lobbies.Include(l => l.Users).SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            var user = _context.Users.Include(u => u.Votes).SingleOrDefault(u => u.UserId == userId);

            if (user == null)
                return NotFound("The specified user does not exist.");

            if (!lobby.Users.Any(u => u.UserId == user.UserId))
                return Forbid();

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


        [HttpPatch]
        [Route("/[controller]/{lobbyId}/close")]
        public IActionResult CloseLobby(Guid lobbyId)
        {
            var lobby = _context.Lobbies.SingleOrDefault(l => l.LobbyId == lobbyId);

            if (lobby == null)
                return NotFound("The specified lobby does not exist.");

            lobby.IsClosed = true;
            _context.SaveChanges();

            _hub.Clients.All.SendAsync("LobbyClosed", lobbyId);

            return NoContent();
        }
    }
}
