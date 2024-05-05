using Microsoft.AspNetCore.Mvc;
using RestOlympe_Server.Data;
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
        public IActionResult GetAllUsers()
        {
            var users = _context.Users.ToArray();

            return new JsonResult(new
            {
                users
            });
        }

        [HttpGet]
        [Route("/[controller]/{username}")]
        public IActionResult GetUserByName(string username)
        {
            var user = _context.Users.SingleOrDefault(u => u.UserName == username);

            if (user == null)
            {
                return NotFound();
            }

            return new JsonResult(new
            {
                user
            });
        }


        [HttpPost]
        [Route("/[controller]")]
        public IActionResult AddUser([MaxLength(32)][Required] string username)
        {
            if (username.Length > 255)
                return BadRequest("Username was over 255 characters");

            var potentialUser = _context.Users.SingleOrDefault(u => u.UserName == username);

            if (potentialUser != null)
            {
                return new StatusCodeResult(409); // Conflict
            }


            _context.Users.Add(new()
            {
                UserId = Guid.NewGuid(),
                UserName = username,
            });
            _context.SaveChanges();


            return Created();
        }

        [HttpDelete]
        [Route("/[controller]/{username}")]
        public IActionResult DeleteUserByName(string username)
        {
            var user = _context.Users.SingleOrDefault(u => u.UserName == username);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
