using InOut.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace YourNamespace.Controllers
{
    [Route("api/allowed-tags")]
    [ApiController]
    public class AllowedTagsController : ControllerBase
    {
        private static List<string> AllowedTags = new List<string>();
        private readonly IHubContext<RfidHub> _hubContext;

        public AllowedTagsController(IHubContext<RfidHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // Get all allowed tags
        [HttpGet]
        public IActionResult GetAllowedTags()
        {
            return Ok(AllowedTags);
        }

        // Add a new tag to the allowed list
        [HttpPost("add")]
        public IActionResult AddTag([FromBody] string epc)
        {
            if (string.IsNullOrWhiteSpace(epc))
            {
                return BadRequest("EPC cannot be empty.");
            }

            if (AllowedTags.Contains(epc))
            {
                return BadRequest("Tag already exists.");
            }

            AllowedTags.Add(epc);

            // Notify all clients about the updated list
            _hubContext.Clients.All.SendAsync("AllowedTagsUpdated", AllowedTags);

            return Ok(new { message = "Tag added successfully." });
        }

        // Remove a tag from the allowed list
        [HttpDelete("remove")]
        public IActionResult RemoveTag([FromQuery] string epc)
        {
            if (string.IsNullOrWhiteSpace(epc))
            {
                return BadRequest("EPC cannot be empty.");
            }

            if (!AllowedTags.Contains(epc))
            {
                return NotFound("Tag not found.");
            }

            AllowedTags.Remove(epc);

            // Notify all clients about the updated list
            _hubContext.Clients.All.SendAsync("AllowedTagsUpdated", AllowedTags);

            return Ok(new { message = "Tag removed successfully." });
        }
    }
}

