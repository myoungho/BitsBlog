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
        public async Task<IActionResult> Get(int postId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;
            var skip = (page - 1) * pageSize;
            var total = await _service.CountByPostIdAsync(postId);
            var comments = await _service.GetCommentsByPostIdPagedAsync(postId, skip, pageSize);
            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(comments);
        }

        public record CreateCommentRequest(string Content);

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public async Task<IActionResult> Post(int postId, [FromBody] CreateCommentRequest req)
        {
            var content = _sanitizer.Sanitize(req.Content ?? string.Empty);
            var loginId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var displayName = User.FindFirstValue(ClaimTypes.Name) ?? loginId;
            int? customerId = null; var cid = User.FindFirstValue("cid");
            if (int.TryParse(cid, out var parsed)) customerId = parsed;
            var created = await _service.CreateAsync(postId, content, loginId, displayName, customerId);
            return Created($"/api/posts/{postId}/comments/{created.Id}", created);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("{commentId}")]
        public async Task<IActionResult> Put(int postId, int commentId, [FromBody] CreateCommentRequest req)
        {
            var existing = await _service.GetByIdAsync(commentId);
            if (existing is null || existing.PostId != postId) return NotFound();
            var isAdmin = User?.IsInRole("Admin") == true;
            var loginId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (!isAdmin && !string.Equals(existing.AuthorLoginId, loginId, StringComparison.OrdinalIgnoreCase))
                return Forbid();
            var content = _sanitizer.Sanitize(req.Content ?? string.Empty);
            var updated = await _service.UpdateAsync(commentId, content);
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
