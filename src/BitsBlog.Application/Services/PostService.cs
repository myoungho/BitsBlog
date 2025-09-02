using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;
using BitsBlog.Application.Interfaces;
using BitsBlog.Domain.Entities;

namespace BitsBlog.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IRepository<Post> _repository;
        public PostService(IRepository<Post> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PostDto>> GetPostsAsync()
        {
            var posts = await _repository.GetAllAsync();
            return posts.Select(p => new PostDto(p.Id, p.Title, p.Content, p.Created));
        }

        public async Task<PostDto> CreateAsync(string title, string content)
        {
            var post = await _repository.InsertAsync(new Post { Title = title, Content = content });
            await _repository.SaveDbContextChangesAsync();
            return new PostDto(post.Id, post.Title, post.Content, post.Created);
        }
    }
}
