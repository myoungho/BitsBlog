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
        private readonly IAdminMaintenanceService _admin;
        public UsersController(ICustomerService customers, IAdminMaintenanceService admin)
        {
            _customers = customers;
            _admin = admin;
        }

        /// <summary>사용자 목록(관리자)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BitsBlog.Application.DTOs.UserDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] BitsBlog.Application.DTOs.UserQueryDto query)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.PageSize < 1) query.PageSize = 10;
            if (query.PageSize > 100) query.PageSize = 100;
            var total = await _customers.CountUsersAsync(query);
            var list = await _customers.ListUsersAsync(query);
            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _customers.GetUserByIdAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        /// <summary>사용자 역할 변경</summary>
        [HttpPut("{id:int}/role")]
        [Consumes("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SetRole(int id, [FromBody] BitsBlog.Application.DTOs.SetUserRoleDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Role)) return BadRequest();
            var ok = await _customers.SetRoleAsync(id, req.Role);
            if (!ok) return NotFound();
            return NoContent();
        }

        /// <summary>사용자 삭제</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id, [FromQuery] string? mode = null)
        {
            var ok = await _admin.DeleteUserAsync(id, string.IsNullOrWhiteSpace(mode) ? "anonymize" : mode!);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
