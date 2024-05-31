using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestOlympe_Server.Data;
using RestOlympe_Server.Hubs;
using RestOlympe_Server.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestOlympe_Server.Controllers
{
    [ApiController]
    public class LobbyController : ControllerBase
    {

        private readonly ILogger<LobbyController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<RestoHub> _hub;

        public LobbyController(
            ILogger<LobbyController> logger,
            ApplicationDbContext context,
            IHubContext<RestoHub> hub)
        {
            _logger = logger;
            _context = context;
            _hub = hub;
        }

        [HttpGet]
        [Route("/[controller]/[action]")]
        public IActionResult ToDeleteSendMessageToClients(string message)
        {
            _hub.Clients.All.SendAsync("ReceiveMessage", message);

            return NoContent();
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
            float voteRadiusKilometers
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var admin = _context.Users
                .Include(u => u.LobbiesAsUser)
                .Include(u => u.LobbiesAsAdmin)
                .SingleOrDefault(u => u.UserId == adminId);

            if (admin == null)
                return NotFound("Admin user does not exist.");

            var newLobby = new LobbyModel()
            {
                AdminId = adminId,
                VoteRadiusKm = voteRadiusKilometers,
                Admin = admin,
                Name = lobbyName,
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
    }
}
