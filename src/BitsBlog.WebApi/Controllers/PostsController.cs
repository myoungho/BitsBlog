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
        public PostsController(IPostService service)
        {
            _service = service;
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
            var post = await _service.CreateAsync(request.Title, request.Content);
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PostDto>> Put(int id, [FromBody] UpdatePostRequest request)
        {
            if (id <= 0) return BadRequest();
            var updated = await _service.UpdateAsync(id, request.Title, request.Content);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        public record CreatePostRequest(string Title, string Content);
        public record UpdatePostRequest(string Title, string Content);
    }
}
