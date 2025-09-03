using System.Collections.Generic;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;
using BitsBlog.Application.Services;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public async Task<IEnumerable<PostDto>> Get() => await _service.GetPostsAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetById(int id)
        {
            var post = await _service.GetByIdAsync(id);
            if (post is null) return NotFound();
            return Ok(post);
        }

        [HttpPost]
        public async Task<ActionResult<PostDto>> Post([FromBody] CreatePostRequest request)
        {
            var safe = _sanitizer.Sanitize(request.Content);
            var post = await _service.CreateAsync(request.Title, safe);
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PostDto>> Put(int id, [FromBody] UpdatePostRequest request)
        {
            if (id <= 0) return BadRequest();
            var safe = _sanitizer.Sanitize(request.Content);
            var updated = await _service.UpdateAsync(id, request.Title, safe);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        public record CreatePostRequest(string Title, string Content);
        public record UpdatePostRequest(string Title, string Content);
    }
}

