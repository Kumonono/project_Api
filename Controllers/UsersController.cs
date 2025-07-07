using Microsoft.AspNetCore.Mvc;
using api.Models;
using api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseService _service;

        public UsersController(DatabaseService service)
        {
            _service = service;
        }

        /*[HttpGet]
        public async Task<ActionResult<List<Users>>> Get() =>
            await _service.GetUsers();
        */
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] Users newUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.CreateUsers(newUser);
            return CreatedAtAction(nameof(GetById), new { id = newUser.UserId }, newUser);
        }
        [HttpPut("{id:length(24)}")]
        [Authorize]
        public async Task<IActionResult> Update(string id, [FromBody] Users selectedUser)
        {
            string userId = User.FindFirstValue("UserId")!;
            if (userId != id)
            {
                return Forbid("You can only update your own account.");
            }
            var exist = await _service.GetUserAsync(id);
            if (exist == null)
            {
                return NotFound();
            }

            selectedUser.UserId = id;
            await _service.UpdateUserAsync(id, selectedUser);

            return NoContent();
        }
        [HttpGet]
        [Authorize]

        public async Task<ActionResult<Users>> GetById()
        {
            string userId = User.FindFirstValue("UserId")!;
            var selectedUser = await _service.GetUserAsync(userId);
            if (selectedUser == null)
            {
                return NotFound();
            }
            return Ok(selectedUser);
        }
    }
}