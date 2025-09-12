using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitsBlog.Application.DTO;
using BitsBlog.Application.Interfaces;
using BitsBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BitsBlog.Application.Common;

namespace BitsBlog.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IRepository<Post> _repository;
        public PostService(IRepository<Post> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PostDto>> GetPostsAsync(System.Threading.CancellationToken ct = default)
        {
            var posts = await _repository.ListAsync(null, ct);
            return posts.Select(p => new PostDto(p.Id, p.Title, p.Content, p.Created)
            {
                AuthorLoginId = p.AuthorLoginId,
                AuthorDisplayName = p.AuthorDisplayName
            });
        }

        public async Task<BitsBlog.Application.DTO.Common.PagedResult<PostDto>> GetPagedAsync(BitsBlog.Application.DTO.PostQueryDto queryDto, System.Threading.CancellationToken ct = default)
        {
            var q = _repository.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(queryDto.Q))
            {
                var term = queryDto.Q.Trim();
                q = q.Where(p => EF.Functions.Like(p.Title, "%" + term + "%"));
            }

            var projected = q.OrderByDescending(c => c.Id).Select(p => new PostDto(p.Id, p.Title, p.Content, p.Created));

            return await _repository.PagedAsync<Post, PostDto>(
                projected,
                queryDto.Page,
                queryDto.PageSize);
        }

        public Task<int> CountAsync(BitsBlog.Application.DTO.PostQueryDto queryDto, System.Threading.CancellationToken ct = default)
        {
            var query = _repository.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(queryDto.Q))
            {
                var term = queryDto.Q.Trim();
                query = query.Where(p => EF.Functions.Like(p.Title, "%" + term + "%"));
            }
            return query.CountAsync(ct);
        }

        public async Task<PostDto> CreateAsync(BitsBlog.Application.DTO.PostCreateDto dto, System.Threading.CancellationToken ct = default)
        {
            Post post = null!;
            await _repository.ExecuteInTransactionAsync(async _ =>
            {
                post = await _repository.InsertAsync(new Post { Title = dto.Title, Content = dto.Content, AuthorLoginId = dto.AuthorLoginId, AuthorDisplayName = dto.AuthorDisplayName, CustomerId = dto.CustomerId }, ct);
            }, ct);
            return new PostDto(post.Id, post.Title, post.Content, post.Created)
            {
                AuthorLoginId = post.AuthorLoginId,
                AuthorDisplayName = post.AuthorDisplayName,
                CustomerId = post.CustomerId
            };
        }

        public async Task<PostDto?> GetByIdAsync(int id, System.Threading.CancellationToken ct = default)
        {
            var post = await _repository.GetByIdAsync(id, ct);
            if (post is null) return null;
            return new PostDto(post.Id, post.Title, post.Content, post.Created)
            {
                AuthorLoginId = post.AuthorLoginId,
                AuthorDisplayName = post.AuthorDisplayName,
                CustomerId = post.CustomerId
            };
        }

        public async Task<PostDto?> UpdateAsync(BitsBlog.Application.DTO.PostUpdateDto dto, System.Threading.CancellationToken ct = default)
        {
            var post = await _repository.GetByIdAsync(dto.Id, ct);
            if (post is null) return null;
            post.Title = dto.Title;
            post.Content = dto.Content;
            await _repository.ExecuteInTransactionAsync(async _ =>
            {
                await _repository.UpdateAsync(post, ct);
            }, ct);
            return new PostDto(post.Id, post.Title, post.Content, post.Created)
            {
                AuthorLoginId = post.AuthorLoginId,
                AuthorDisplayName = post.AuthorDisplayName,
                CustomerId = post.CustomerId
            };
        }

        public async Task<bool> DeleteAsync(int id, System.Threading.CancellationToken ct = default)
        {
            var post = await _repository.GetByIdAsync(id, ct);
            if (post is null) return false;
            await _repository.ExecuteInTransactionAsync(async _ =>
            {
                await _repository.DeleteAsync(post, ct);
            }, ct);
            return true;
        }
    }
}
