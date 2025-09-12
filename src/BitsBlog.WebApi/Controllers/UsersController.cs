using System.ComponentModel.DataAnnotations;
using BitsBlog.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BitsBlog.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly ICustomerService _customers;
        public UsersController(ICustomerService customers)
        {
            _customers = customers;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? q = null, [FromQuery] string? sort = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;
            var skip = (page - 1) * pageSize;
            var total = await _customers.CountUsersAsync(q);
            var list = await _customers.ListUsersAsync(skip, pageSize, q, sort);
            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _customers.GetUserByIdAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        public record SetRoleRequest([Required] string Role);

        [HttpPut("{id:int}/role")]
        public async Task<IActionResult> SetRole(int id, [FromBody] SetRoleRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Role)) return BadRequest();
            var ok = await _customers.SetRoleAsync(id, req.Role);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _customers.DeleteUserAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
