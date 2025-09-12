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
            var posts = await _repository.ListAsync();
            return posts.Select(p => new PostDto(p.Id, p.Title, p.Content, p.Created)
            {
                AuthorLoginId = p.AuthorLoginId,
                AuthorDisplayName = p.AuthorDisplayName
            });
        }

        public async Task<IReadOnlyList<PostDto>> GetPagedAsync(BitsBlog.Application.DTOs.PostQueryDto queryDto)
        {
            var skip = (queryDto.Page - 1) * queryDto.PageSize;
            if (skip < 0) skip = 0; var take = queryDto.PageSize <= 0 ? 10 : queryDto.PageSize;
            var query = _repository.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(queryDto.Q))
            {
                var term = queryDto.Q.Trim();
                query = query.Where(p => EF.Functions.Like(p.Title, "%" + term + "%") || EF.Functions.Like(p.Content, "%" + term + "%"));
            }
            if (string.Equals(queryDto.Sort, "created_asc", System.StringComparison.OrdinalIgnoreCase))
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

        public Task<int> CountAsync(BitsBlog.Application.DTOs.PostQueryDto queryDto)
        {
            var query = _repository.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(queryDto.Q))
            {
                var term = queryDto.Q.Trim();
                query = query.Where(p => EF.Functions.Like(p.Title, "%" + term + "%") || EF.Functions.Like(p.Content, "%" + term + "%"));
            }
            return query.CountAsync();
        }

        public async Task<PostDto> CreateAsync(BitsBlog.Application.DTOs.PostCreateDto dto)
        {
            Post post = null!;
            await _repository.ExecuteInTransactionAsync(async _ =>
            {
                post = await _repository.InsertAsync(new Post { Title = dto.Title, Content = dto.Content, AuthorLoginId = dto.AuthorLoginId, AuthorDisplayName = dto.AuthorDisplayName, CustomerId = dto.CustomerId });
            });
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

        public async Task<PostDto?> UpdateAsync(BitsBlog.Application.DTOs.PostUpdateDto dto)
        {
            var post = await _repository.GetByIdAsync(dto.Id);
            if (post is null) return null;
            post.Title = dto.Title;
            post.Content = dto.Content;
            await _repository.ExecuteInTransactionAsync(async _ =>
            {
                await _repository.UpdateAsync(post);
            });
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
            await _repository.ExecuteInTransactionAsync(async _ =>
            {
                await _repository.DeleteAsync(post);
            });
            return true;
        }
    }
}
