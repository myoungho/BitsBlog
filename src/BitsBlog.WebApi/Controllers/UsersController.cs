using System.ComponentModel.DataAnnotations;
using BitsBlog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BitsBlog.Application.DTO;
using BitsBlog.Application.DTO.Common;

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
        [ProducesResponseType(typeof(PagedResult<UserDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] UserQueryDto query)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.PageSize < 1) query.PageSize = 10;
            if (query.PageSize > 100) query.PageSize = 100;
            var ct = HttpContext.RequestAborted;
            var paged = await _customers.ListUsersAsync(query, ct);
            return Ok(paged);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _customers.GetUserByIdAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        /// <summary>사용자 역할 변경</summary>
        [HttpPut("role")]
        [Consumes("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SetRole([FromBody] SetUserRoleDto req)
        {
            if (req.Id <= 0 || string.IsNullOrWhiteSpace(req.Role)) return BadRequest();
            var ok = await _customers.SetRoleAsync(req.Id, req.Role, HttpContext.RequestAborted);
            if (!ok) return NotFound();
            return NoContent();
        }

        /// <summary>사용자 삭제</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id, [FromQuery] string? mode = null)
        {
            var ok = await _admin.DeleteUserAsync(id, string.IsNullOrWhiteSpace(mode) ? "anonymize" : mode!, HttpContext.RequestAborted);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
