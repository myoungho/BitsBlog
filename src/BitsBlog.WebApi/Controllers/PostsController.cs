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

        /// <summary>게시글 목록 조회</summary>
        /// <param name="query">페이지/검색/정렬 파라미터</param>
        /// <returns>게시글 목록</returns>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PostDto>), 200)]
        public async Task<IEnumerable<PostDto>> Get([FromQuery] BitsBlog.Application.DTOs.PostQueryDto query)
        {
            if (query.Page < 1) query.Page = 1;
            if (query.PageSize < 1) query.PageSize = 10;
            if (query.PageSize > 100) query.PageSize = 100;
            var ct = HttpContext.RequestAborted;
            var total = await _service.CountAsync(query, ct);
            var items = await _service.GetPagedAsync(query, ct);
            Response.Headers["X-Total-Count"] = total.ToString();
            return items;
        }

        // Backward-compatible overload for tests invoking method directly
        // Not an action (no attributes)
        public async Task<IEnumerable<PostDto>> Get(int page, int pageSize, string? q, string? sort)
        {
            var query = new BitsBlog.Application.DTOs.PostQueryDto { Page = page, PageSize = pageSize, Q = q, Sort = sort };
            var total = await _service.CountAsync(query);
            var items = await _service.GetPagedAsync(query);
            Response.Headers["X-Total-Count"] = total.ToString();
            return items;
        }

        // Parameterless overload for tests
        public Task<IEnumerable<PostDto>> Get()
        {
            return Get(new BitsBlog.Application.DTOs.PostQueryDto());
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetById(int id)
        {
            var post = await _service.GetByIdAsync(id);
            if (post is null) return NotFound();
            return Ok(post);
        }

        /// <summary>게시글 생성</summary>
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(PostDto), 201)]
        public async Task<ActionResult<PostDto>> Post([FromBody] BitsBlog.Application.DTOs.PostCreateDto body)
        {
            var safe = _sanitizer.Sanitize(body.Content);
            var loginId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var displayName = User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            int? customerId = null;
            var cid = User?.FindFirst("cid")?.Value;
            if (int.TryParse(cid, out var parsed)) customerId = parsed;
            var dto = new BitsBlog.Application.DTOs.PostCreateDto { Title = body.Title, Content = safe, AuthorLoginId = loginId, AuthorDisplayName = displayName, CustomerId = customerId };
            var post = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
        }

        // Non-action wrapper for tests using old request type
        public Task<ActionResult<PostDto>> Post(CreatePostRequest req)
        {
            return Post(new BitsBlog.Application.DTOs.PostCreateDto { Title = req.Title, Content = req.Content });
        }

        /// <summary>게시글 수정</summary>
        [Authorize(Roles = "User,Admin")]
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(PostDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PostDto>> Put(int id, [FromBody] BitsBlog.Application.DTOs.PostUpdateDto body)
        {
            if (id <= 0) return BadRequest();
            // Authorize: only author or admin can update
            var existing = await _service.GetByIdAsync(id, HttpContext.RequestAborted);
            if (existing is null) return NotFound();
            var isAdmin = User?.IsInRole("Admin") == true;
            var loginId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!isAdmin)
            {
                if (!string.Equals(existing.AuthorLoginId, loginId, System.StringComparison.OrdinalIgnoreCase))
                    return Forbid();
            }
            var safe = _sanitizer.Sanitize(body.Content);
            var updated = await _service.UpdateAsync(new BitsBlog.Application.DTOs.PostUpdateDto { Id = id, Title = body.Title, Content = safe }, HttpContext.RequestAborted);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        // Non-action wrapper for tests using old request type
        public Task<ActionResult<PostDto>> Put(int id, UpdatePostRequest req)
        {
            return Put(id, new BitsBlog.Application.DTOs.PostUpdateDto { Id = id, Title = req.Title, Content = req.Content });
        }

        /// <summary>게시글 삭제</summary>
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

