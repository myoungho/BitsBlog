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
        public async Task<IActionResult> Get(int postId)
        {
            var comments = await _service.GetCommentsByPostIdAsync(postId);
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
    }
}
