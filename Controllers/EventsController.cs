using Microsoft.AspNetCore.Mvc;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly DatabaseService _service;

        public EventsController(DatabaseService service)
        {
            _service = service;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<Events>>> GetEventsByUser()
        {
            string userId = User.FindFirstValue("UserId")!;
            var userEvents = await _service.GetEventsByUser(userId);
            return Ok(userEvents);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Events newEvent)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string userId = User.FindFirstValue("UserId")!;
            newEvent.UserId = userId;

            await _service.CreateEvent(newEvent);
            return CreatedAtAction(nameof(GetById), new { id = newEvent.EventId }, newEvent);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, [FromBody] Events updatedEvent)
        {
            string userId = User.FindFirstValue("UserId")!;
            var existingEvent = await _service.GetEventAsync(id);

            if (existingEvent == null)
                return NotFound();

            if (existingEvent.UserId != userId)
                return Forbid("You can only update your own events.");

            updatedEvent.UserId = userId;
            updatedEvent.EventId = id;

            await _service.UpdateEventAsync(id, updatedEvent);

            return NoContent();
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Events>> GetById(string id) //szurjuk hogy sajatja e 
        {
            string userId = User.FindFirstValue("UserId")!;
            var eventItem = await _service.GetEventAsync(id);

            if (eventItem == null)
                return NotFound();

            if (eventItem.UserId != userId)
                return Forbid("You can only access your own events.");

            return Ok(eventItem);
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            string userId = User.FindFirstValue("UserId")!;
            var eventItem = await _service.GetEventAsync(id);

            if (eventItem == null)
                return NotFound();

            if (eventItem.UserId != userId)
                return Forbid("You can only delete your own events.");

            var deleted = await _service.DeleteEventAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}