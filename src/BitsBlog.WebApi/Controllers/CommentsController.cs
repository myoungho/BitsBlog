using System.Threading.Tasks;
using BitsBlog.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Get(int postId, [FromQuery] BitsBlog.Application.DTOs.CommentQueryDto query)
        {
            if (postId <= 0) return BadRequest();
            query.PostId = postId;
            if (query.Page < 1) query.Page = 1;
            if (query.PageSize < 1) query.PageSize = 10;
            if (query.PageSize > 100) query.PageSize = 100;
            var total = await _service.CountAsync(query);
            var comments = await _service.GetPagedAsync(query);
            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(comments);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public async Task<IActionResult> Post(int postId, [FromBody] BitsBlog.Application.DTOs.CommentCreateDto body)
        {
            var content = _sanitizer.Sanitize(body.Content ?? string.Empty);
            var loginId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var displayName = User.FindFirstValue(ClaimTypes.Name) ?? loginId;
            int? customerId = null; var cid = User.FindFirstValue("cid");
            if (int.TryParse(cid, out var parsed)) customerId = parsed;
            var dto = new BitsBlog.Application.DTOs.CommentCreateDto { PostId = postId, Content = content, AuthorLoginId = loginId, AuthorDisplayName = displayName, CustomerId = customerId };
            var created = await _service.CreateAsync(dto);
            return Created($"/api/posts/{postId}/comments/{created.Id}", created);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("{commentId}")]
        public async Task<IActionResult> Put(int postId, int commentId, [FromBody] BitsBlog.Application.DTOs.CommentUpdateDto body)
        {
            var existing = await _service.GetByIdAsync(commentId);
            if (existing is null || existing.PostId != postId) return NotFound();
            var isAdmin = User?.IsInRole("Admin") == true;
            var loginId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (!isAdmin && !string.Equals(existing.AuthorLoginId, loginId, StringComparison.OrdinalIgnoreCase))
                return Forbid();
            var content = _sanitizer.Sanitize(body.Content ?? string.Empty);
            var updated = await _service.UpdateAsync(new BitsBlog.Application.DTOs.CommentUpdateDto { CommentId = commentId, Content = content });
            return Ok(updated);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> Delete(int postId, int commentId)
        {
            var existing = await _service.GetByIdAsync(commentId);
            if (existing is null || existing.PostId != postId) return NotFound();
            var isAdmin = User?.IsInRole("Admin") == true;
            var loginId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (!isAdmin && !string.Equals(existing.AuthorLoginId, loginId, StringComparison.OrdinalIgnoreCase))
                return Forbid();
            var ok = await _service.DeleteAsync(commentId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
