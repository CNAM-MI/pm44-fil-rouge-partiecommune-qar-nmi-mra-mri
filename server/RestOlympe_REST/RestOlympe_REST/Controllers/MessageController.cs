using Microsoft.AspNetCore.Mvc;
using RestOlympe_REST.Data;
using System.ComponentModel.DataAnnotations;

namespace RestOlympe_REST.Controllers
{
    [ApiController]
    public class MessageController : ControllerBase
    {

        private readonly ILogger<MessageController> _logger;
        private readonly ApplicationDbContext _context;

        public MessageController(ILogger<MessageController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [Route("/[controller]/[action]")]
        public IActionResult GetAllMessages()
        {
            var messages = _context.Messages.ToArray();

            return new JsonResult(new
            {
                messages
            });
        }



        [HttpPost]
        [Route("/[controller]/[action]/{message}")]
        public IActionResult AddMessage([MaxLength(255)] string message)
        {
            if (message.Length > 255)
                return BadRequest("Message was over 255 characters");
            _context.Messages.Add(new()
            {
                UserId = 1,
                Message = message,
                CreatedAt = DateTime.UtcNow,
            });
            _context.SaveChanges();
            return Created();
        }
    }
}
