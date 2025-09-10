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
            return posts.Select(p => new PostDto(p.Id, p.Title, p.Content, p.Created)
            {
                AuthorLoginId = p.AuthorLoginId,
                AuthorDisplayName = p.AuthorDisplayName
            });
        }

        public async Task<PostDto> CreateAsync(string title, string content, string? authorLoginId = null, string? authorDisplayName = null, int? customerId = null)
        {
            var post = await _repository.InsertAsync(new Post { Title = title, Content = content, AuthorLoginId = authorLoginId, AuthorDisplayName = authorDisplayName, CustomerId = customerId });
            await _repository.SaveDbContextChangesAsync();
            return new PostDto(post.Id, post.Title, post.Content, post.Created)
            {
                AuthorLoginId = post.AuthorLoginId,
                AuthorDisplayName = post.AuthorDisplayName,
                CustomerId = post.CustomerId
            };
        }

        public async Task<PostDto?> GetByIdAsync(int id)
        {
            var post = await _repository.GetByIdAsync(id);
            if (post is null) return null;
            return new PostDto(post.Id, post.Title, post.Content, post.Created)
            {
                AuthorLoginId = post.AuthorLoginId,
                AuthorDisplayName = post.AuthorDisplayName,
                CustomerId = post.CustomerId
            };
        }

        public async Task<PostDto?> UpdateAsync(int id, string title, string content)
        {
            var post = await _repository.GetByIdAsync(id);
            if (post is null) return null;
            post.Title = title;
            post.Content = content;
            await _repository.UpdateAsync(post);
            await _repository.SaveDbContextChangesAsync();
            return new PostDto(post.Id, post.Title, post.Content, post.Created)
            {
                AuthorLoginId = post.AuthorLoginId,
                AuthorDisplayName = post.AuthorDisplayName,
                CustomerId = post.CustomerId
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var post = await _repository.GetByIdAsync(id);
            if (post is null) return false;
            await _repository.DeleteAsync(post);
            await _repository.SaveDbContextChangesAsync();
            return true;
        }
    }
}
