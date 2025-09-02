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

        [HttpPost]
        public async Task<ActionResult<PostDto>> Post([FromBody] CreatePostRequest request)
        {
            var post = await _service.CreateAsync(request.Title, request.Content);
            return CreatedAtAction(nameof(Get), new { id = post.Id }, post);
        }

        public record CreatePostRequest(string Title, string Content);
    }
}
