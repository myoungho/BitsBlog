using System.Collections.Generic;
using System.Threading.Tasks;
using BitsBlog.Application.DTO;
using BitsBlog.Application.Interfaces;
using BitsBlog.Application.DTO.Common;
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

        /// <summary>Retrieve post list</summary>
        /// <param name="query">Paging / search / sorting parameters</param>
        /// <returns>List of posts</returns>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<PostDto>), 200)]
        public async Task<PagedResult<PostDto>> Get([FromQuery] BitsBlog.Application.DTO.PostQueryDto query)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.PageSize < 1) query.PageSize = 10;
            if (query.PageSize > 100) query.PageSize = 100;
            var ct = HttpContext.RequestAborted;
            var paged = await _service.GetPagedAsync(query, ct);
            return paged;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetById(int id)
        {
            var post = await _service.GetByIdAsync(id);
            if (post is null) return NotFound();
            return Ok(post);
        }

        /// <summary>Create post</summary>
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(PostDto), 201)]
        public async Task<ActionResult<PostDto>> Post([FromBody] BitsBlog.Application.DTO.PostCreateDto body)
        {
            var safe = _sanitizer.Sanitize(body.Content);
            var loginId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var displayName = User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            int? customerId = null;
            var cid = User?.FindFirst("cid")?.Value;
            if (int.TryParse(cid, out var parsed)) customerId = parsed;
            var dto = new BitsBlog.Application.DTO.PostCreateDto { Title = body.Title, Content = safe, AuthorLoginId = loginId, AuthorDisplayName = displayName, CustomerId = customerId };
            var post = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
        }

        /// <summary>Update post</summary>
        [Authorize(Roles = "User,Admin")]
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(PostDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PostDto>> Put([FromBody] BitsBlog.Application.DTO.PostUpdateDto body)
        {
            if (body is null || body.Id <= 0) return BadRequest();
            // Authorize: only author or admin can update
            var existing = await _service.GetByIdAsync(body.Id, HttpContext.RequestAborted);
            if (existing is null) return NotFound();
            var isAdmin = User?.IsInRole("Admin") == true;
            var loginId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!isAdmin)
            {
                if (!string.Equals(existing.AuthorLoginId, loginId, System.StringComparison.OrdinalIgnoreCase))
                    return Forbid();
            }
            var safe = _sanitizer.Sanitize(body.Content ?? string.Empty);
            var updated = await _service.UpdateAsync(new BitsBlog.Application.DTO.PostUpdateDto { Id = body.Id, Title = body.Title, Content = safe }, HttpContext.RequestAborted);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        /// <summary>Update post (id in route) - compatibility overload for tests</summary>
        [Authorize(Roles = "User,Admin")]
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(PostDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PostDto>> Put(int id, [FromBody] BitsBlog.Application.DTO.PostUpdateDto body)
        {
            if (body is null) return BadRequest();
            body = new BitsBlog.Application.DTO.PostUpdateDto { Id = id, Title = body.Title, Content = body.Content };
            return await Put(body);
        }

        /// <summary>Delete post</summary>
        [Authorize(Roles = "User,Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
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

        // Backward-compatible request records for tests
        public record CreatePostRequest(string Title, string Content);
        public record UpdatePostRequest(string Title, string Content);
    }
}
