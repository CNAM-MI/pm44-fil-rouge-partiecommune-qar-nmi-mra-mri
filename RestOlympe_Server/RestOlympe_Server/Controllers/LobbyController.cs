using Microsoft.AspNetCore.Mvc;
using RestOlympe_Server.Data;
using System.ComponentModel.DataAnnotations;

namespace RestOlympe_Server.Controllers
{
    [ApiController]
    public class LobbyController : ControllerBase
    {

        private readonly ILogger<LobbyController> _logger;
        private readonly ApplicationDbContext _context;

        public LobbyController(ILogger<LobbyController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [Route("/[controller]")]
        public IActionResult GetAllLobbies()
        {
            var lobbies = _context.Lobbies.ToArray();

            return new JsonResult(new
            {
                lobbies
            });
        }
    }
}
