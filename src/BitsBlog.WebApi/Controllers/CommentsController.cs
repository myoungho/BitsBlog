using System.Threading.Tasks;
using BitsBlog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BitsBlog.Application.DTO;
using System.Collections.Generic;

namespace BitsBlog.WebApi.Controllers
{
    [ApiController]
    [Route("api/posts/{postId:int}/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _service;
        private readonly Ganss.Xss.IHtmlSanitizer _sanitizer;
        public CommentsController(ICommentService service, Ganss.Xss.IHtmlSanitizer sanitizer)
        {
            _service = service;
            _sanitizer = sanitizer;
        }

        /// <summary>코멘트 목록 조회</summary>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CommentDto>), 200)]
        public async Task<IActionResult> Get(int postId, [FromQuery] CommentQueryDto query)
        {
            if (postId <= 0) return BadRequest();
            query.PostId = postId;
            if (query.Page < 1) query.Page = 1;
            if (query.PageSize < 1) query.PageSize = 10;
            if (query.PageSize > 100) query.PageSize = 100;
            var ct = HttpContext.RequestAborted;
            var total = await _service.CountAsync(query, ct);
            var comments = await _service.GetPagedAsync(query, ct);
            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(comments);
        }

        /// <summary>코멘트 작성</summary>
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CommentDto), 201)]
        public async Task<IActionResult> Post(int postId, [FromBody] CommentCreateDto body)
        {
            var content = _sanitizer.Sanitize(body.Content ?? string.Empty);
            var loginId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var displayName = User.FindFirstValue(ClaimTypes.Name) ?? loginId;
            int? customerId = null; var cid = User.FindFirstValue("cid");
            if (int.TryParse(cid, out var parsed)) customerId = parsed;
            var dto = new CommentCreateDto { PostId = postId, Content = content, AuthorLoginId = loginId, AuthorDisplayName = displayName, CustomerId = customerId };
            var created = await _service.CreateAsync(dto);
            return Created($"/api/posts/{postId}/comments/{created.Id}", created);
        }

        /// <summary>코멘트 수정</summary>
        [Authorize(Roles = "User,Admin")]
        [HttpPut("{commentId}")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CommentDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Put(int postId, int commentId, [FromBody] CommentUpdateDto body)
        {
            var existing = await _service.GetByIdAsync(commentId, HttpContext.RequestAborted);
            if (existing is null || existing.PostId != postId) return NotFound();
            var isAdmin = User?.IsInRole("Admin") == true;
            var loginId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (!isAdmin && !string.Equals(existing.AuthorLoginId, loginId, StringComparison.OrdinalIgnoreCase))
                return Forbid();
            var content = _sanitizer.Sanitize(body.Content ?? string.Empty);
            var updated = await _service.UpdateAsync(new CommentUpdateDto { CommentId = commentId, Content = content }, HttpContext.RequestAborted);
            return Ok(updated);
        }

        /// <summary>코멘트 삭제</summary>
        [Authorize(Roles = "User,Admin")]
        [HttpDelete("{commentId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int postId, int commentId)
        {
            var existing = await _service.GetByIdAsync(commentId, HttpContext.RequestAborted);
            if (existing is null || existing.PostId != postId) return NotFound();
            var isAdmin = User?.IsInRole("Admin") == true;
            var loginId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (!isAdmin && !string.Equals(existing.AuthorLoginId, loginId, StringComparison.OrdinalIgnoreCase))
                return Forbid();
            var ok = await _service.DeleteAsync(commentId, HttpContext.RequestAborted);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}