using System.Threading.Tasks;
using BitsBlog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BitsBlog.Application.DTO;
using System.Collections.Generic;
using BitsBlog.Application.DTO.Common;

namespace BitsBlog.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _service;
        private readonly Ganss.Xss.IHtmlSanitizer _sanitizer;
        public CommentsController(ICommentService service, Ganss.Xss.IHtmlSanitizer sanitizer)
        {
            _service = service;
            _sanitizer = sanitizer;
        }

        /// <summary>Retrieve paged list of comments</summary>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<CommentDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] CommentQueryDto query)
        {
            if (query.PostId <= 0) return BadRequest();
            if (query.Page < 1) query.Page = 1;
            if (query.PageSize < 1) query.PageSize = 10;
            if (query.PageSize > 100) query.PageSize = 100;
            var ct = HttpContext.RequestAborted;
            var paged = await _service.GetPagedAsync(query, ct);
            return Ok(paged);
        }

        /// <summary>Create a comment</summary>
        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CommentDto), 201)]
        public async Task<IActionResult> Post([FromBody] CommentCreateDto body)
        {
            var content = _sanitizer.Sanitize(body.Content ?? string.Empty);
            var loginId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var displayName = User.FindFirstValue(ClaimTypes.Name) ?? loginId;
            int? customerId = null; var cid = User.FindFirstValue("cid");
            if (int.TryParse(cid, out var parsed) && parsed > 0) customerId = parsed;
            var dto = new CommentCreateDto { PostId = body.PostId, Content = content, AuthorLoginId = loginId, AuthorDisplayName = displayName, CustomerId = customerId };
            var created = await _service.CreateAsync(dto);
            return Created($"/api/comments/{created.Id}", created);
        }

        /// <summary>Update a comment</summary>
        [Authorize(Roles = "User,Admin")]
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CommentDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Put([FromBody] CommentUpdateDto body)
        {
            var existing = await _service.GetByIdAsync(body.CommentId, HttpContext.RequestAborted);
            if (existing is null || (body.PostId > 0 && existing.PostId != body.PostId)) return NotFound();
            var isAdmin = User?.IsInRole("Admin") == true;
            var loginId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (!isAdmin && !string.Equals(existing.AuthorLoginId, loginId, StringComparison.OrdinalIgnoreCase))
                return Forbid();
            var content = _sanitizer.Sanitize(body.Content ?? string.Empty);
            var updated = await _service.UpdateAsync(new CommentUpdateDto { PostId = body.PostId, CommentId = body.CommentId, Content = content }, HttpContext.RequestAborted);
            return Ok(updated);
        }

        /// <summary>Delete a comment</summary>
        [Authorize(Roles = "User,Admin")]
        [HttpDelete("{commentId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int commentId)
        {
            var existing = await _service.GetByIdAsync(commentId, HttpContext.RequestAborted);
            if (existing is null) return NotFound();
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
