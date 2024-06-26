using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestOlympe_Server.Data;
using RestOlympe_Server.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace RestOlympe_Server.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly ILogger<UserController> _logger;
        private readonly ApplicationDbContext _context;

        public UserController(ILogger<UserController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [Route("/[controller]")]
        [ProducesResponseType(200, StatusCode = 200, Type = typeof(UserModel[]))]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users.ToArray();

            return new JsonResult(new
            {
                users
            });
        }

        [HttpGet]
        [Route("/[controller]/{userId}")]
        [ProducesResponseType(200, StatusCode = 200, Type = typeof(UserModel))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetUser([Required] Guid userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _context.Users.SingleOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            return new JsonResult(user);
        }


        [HttpPost]
        [Route("/[controller]")]
        [ProducesResponseType(201, StatusCode = 200, Type = typeof(UserModel))]
        [ProducesResponseType(400)]
        public IActionResult AddUser([MaxLength(32)][Required][FromForm] string username)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newUser = new UserModel()
            {
                UserId = Guid.NewGuid(),
                UserName = username,
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Created($"/user/{newUser.UserId}", newUser);
        }

        [HttpDelete]
        [Route("/[controller]/{userId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult DeleteUser([Required] Guid userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _context.Users.SingleOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpGet]
        [Route("/[controller]/{userId}/lobby")]
        [ProducesResponseType(200, StatusCode = 200, Type = typeof(LobbyModel[]))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetLobbies([Required] Guid userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _context.Users.Include(u => u.LobbiesAsUser).SingleOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            return new JsonResult(new
            {
                lobbies = user.LobbiesAsUser
            });
        }
    }
}
