using System.Collections.Generic;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;
using BitsBlog.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BitsBlog.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _service;
        private readonly Ganss.Xss.IHtmlSanitizer _sanitizer;
        public PostsController(IPostService service, Ganss.Xss.IHtmlSanitizer sanitizer)
        {
            _service = service;
            _sanitizer = sanitizer;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IEnumerable<PostDto>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? q = null, [FromQuery] string? sort = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;
            var skip = (page - 1) * pageSize;
            var total = await _service.CountAsync(q);
            var items = await _service.GetPostsPagedAsync(skip, pageSize, q, sort);
            Response.Headers["X-Total-Count"] = total.ToString();
            return items;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetById(int id)
        {
            var post = await _service.GetByIdAsync(id);
            if (post is null) return NotFound();
            return Ok(post);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public async Task<ActionResult<PostDto>> Post([FromBody] CreatePostRequest request)
        {
            var safe = _sanitizer.Sanitize(request.Content);
            var loginId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var displayName = User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            int? customerId = null;
            var cid = User?.FindFirst("cid")?.Value;
            if (int.TryParse(cid, out var parsed)) customerId = parsed;
            var post = await _service.CreateAsync(request.Title, safe, loginId, displayName, customerId);
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<PostDto>> Put(int id, [FromBody] UpdatePostRequest request)
        {
            if (id <= 0) return BadRequest();
            // Authorize: only author or admin can update
            var existing = await _service.GetByIdAsync(id);
            if (existing is null) return NotFound();
            var isAdmin = User?.IsInRole("Admin") == true;
            var loginId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!isAdmin)
            {
                if (!string.Equals(existing.AuthorLoginId, loginId, System.StringComparison.OrdinalIgnoreCase))
                    return Forbid();
            }
            var safe = _sanitizer.Sanitize(request.Content);
            var updated = await _service.UpdateAsync(id, request.Title, safe);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            // Authorize: only author or admin can delete
            var existing = await _service.GetByIdAsync(id);
            if (existing is null) return NotFound();
            var isAdmin = User?.IsInRole("Admin") == true;
            var loginId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!isAdmin)
            {
                if (!string.Equals(existing.AuthorLoginId, loginId, System.StringComparison.OrdinalIgnoreCase))
                    return Forbid();
            }
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        public record CreatePostRequest(string Title, string Content);
        public record UpdatePostRequest(string Title, string Content);
    }
}

