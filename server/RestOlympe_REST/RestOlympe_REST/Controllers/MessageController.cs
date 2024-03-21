using Microsoft.AspNetCore.Mvc;
using RestOlympe_REST.Data;

namespace RestOlympe_REST.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {

        private readonly ILogger<MessageController> _logger;
        private readonly ApplicationDbContext _context;

        public MessageController(ILogger<MessageController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetAllMessages")]
        public IActionResult GetAllMessages()
        {
            var messages = _context.Messages.ToArray();

            return new JsonResult(new
            {
                messages
            });
        }
    }
}
