using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;
using BitsBlog.Application.Interfaces;
using BitsBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
            var posts = await _repository.AsNoTracking()
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            return posts.Select(p => new PostDto(p.Id, p.Title, p.Content, p.Created)
            {
                AuthorLoginId = p.AuthorLoginId,
                AuthorDisplayName = p.AuthorDisplayName
            });
        }

        public async Task<IReadOnlyList<PostDto>> GetPostsPagedAsync(int skip, int take, string? q = null, string? sort = null)
        {
            var query = _repository.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(p => EF.Functions.Like(p.Title, "%" + term + "%") || EF.Functions.Like(p.Content, "%" + term + "%"));
            }
            if (string.Equals(sort, "created_asc", System.StringComparison.OrdinalIgnoreCase))
                query = query.OrderBy(p => p.Created);
            else
                query = query.OrderByDescending(p => p.Created);

            var sel = query
                .Skip(skip)
                .Take(take)
                .Select(p => new PostDto(p.Id, p.Title, p.Content, p.Created)
                {
                    AuthorLoginId = p.AuthorLoginId,
                    AuthorDisplayName = p.AuthorDisplayName,
                    CustomerId = p.CustomerId
                });
            return await sel.ToListAsync();
        }

        public Task<int> CountAsync(string? q = null)
        {
            var query = _repository.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(p => EF.Functions.Like(p.Title, "%" + term + "%") || EF.Functions.Like(p.Content, "%" + term + "%"));
            }
            return query.CountAsync();
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
