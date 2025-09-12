using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;
using BitsBlog.Application.Interfaces;
using BitsBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BitsBlog.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly IRepository<Comment> _repository;
        public CommentService(IRepository<Comment> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByPostIdAsync(int postId, System.Threading.CancellationToken ct = default)
        {
            var q = _repository.AsNoTracking()
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.Id);
            var list = await q.ToListAsync(ct);
            return list.Select(c => new CommentDto(c.Id, c.PostId, c.Content, c.Created)
            {
                AuthorLoginId = c.AuthorLoginId,
                AuthorDisplayName = c.AuthorDisplayName
            });
        }

        public async Task<IReadOnlyList<CommentDto>> GetPagedAsync(BitsBlog.Application.DTOs.CommentQueryDto queryDto, System.Threading.CancellationToken ct = default)
        {
            var skip = (queryDto.Page - 1) * queryDto.PageSize; if (skip < 0) skip = 0;
            var take = queryDto.PageSize <= 0 ? 10 : queryDto.PageSize;
            var query = _repository.AsNoTracking().Where(c => c.PostId == queryDto.PostId);
            if (!string.IsNullOrWhiteSpace(queryDto.Q))
            {
                var term = queryDto.Q.Trim();
                query = query.Where(c => EF.Functions.Like(c.Content, "%" + term + "%") || EF.Functions.Like(c.AuthorDisplayName!, "%" + term + "%"));
            }
            if (string.Equals(queryDto.Sort, "created_asc", System.StringComparison.OrdinalIgnoreCase))
                query = query.OrderBy(c => c.Created);
            else
                query = query.OrderByDescending(c => c.Created);

            var sel = query
                .Skip(skip)
                .Take(take)
                .Select(c => new CommentDto(c.Id, c.PostId, c.Content, c.Created)
                {
                    AuthorLoginId = c.AuthorLoginId,
                    AuthorDisplayName = c.AuthorDisplayName,
                    CustomerId = c.CustomerId
                });
            return await sel.ToListAsync(ct);
        }

        public Task<int> CountAsync(BitsBlog.Application.DTOs.CommentQueryDto queryDto, System.Threading.CancellationToken ct = default)
        {
            var query = _repository.AsNoTracking().Where(c => c.PostId == queryDto.PostId);
            if (!string.IsNullOrWhiteSpace(queryDto.Q))
            {
                var term = queryDto.Q.Trim();
                query = query.Where(c => EF.Functions.Like(c.Content, "%" + term + "%") || EF.Functions.Like(c.AuthorDisplayName!, "%" + term + "%"));
            }
            return query.CountAsync(ct);
        }

        public async Task<CommentDto> CreateAsync(BitsBlog.Application.DTOs.CommentCreateDto dto, System.Threading.CancellationToken ct = default)
        {
            var comment = new Comment { PostId = dto.PostId, Content = dto.Content, AuthorLoginId = dto.AuthorLoginId, AuthorDisplayName = dto.AuthorDisplayName, CustomerId = dto.CustomerId };
            Comment created = null!;
            await _repository.ExecuteInTransactionAsync(async _ =>
            {
                created = await _repository.InsertAsync(comment, ct);
            }, ct);
            return new CommentDto(created.Id, created.PostId, created.Content, created.Created)
            {
                AuthorLoginId = created.AuthorLoginId,
                AuthorDisplayName = created.AuthorDisplayName,
                CustomerId = created.CustomerId
            };
        }

        public async Task<CommentDto?> GetByIdAsync(int id, System.Threading.CancellationToken ct = default)
        {
            var c = await _repository.GetByIdAsync(id, ct);
            if (c is null) return null;
            return new CommentDto(c.Id, c.PostId, c.Content, c.Created)
            {
                AuthorLoginId = c.AuthorLoginId,
                AuthorDisplayName = c.AuthorDisplayName,
                CustomerId = c.CustomerId
            };
        }

        public async Task<CommentDto?> UpdateAsync(BitsBlog.Application.DTOs.CommentUpdateDto dto, System.Threading.CancellationToken ct = default)
        {
            var c = await _repository.GetByIdAsync(dto.CommentId, ct);
            if (c is null) return null;
            c.Content = dto.Content;
            await _repository.ExecuteInTransactionAsync(async _ =>
            {
                await _repository.UpdateAsync(c, ct);
            }, ct);
            return new CommentDto(c.Id, c.PostId, c.Content, c.Created)
            {
                AuthorLoginId = c.AuthorLoginId,
                AuthorDisplayName = c.AuthorDisplayName,
                CustomerId = c.CustomerId
            };
        }

        public async Task<bool> DeleteAsync(int id, System.Threading.CancellationToken ct = default)
        {
            var c = await _repository.GetByIdAsync(id, ct);
            if (c is null) return false;
            await _repository.ExecuteInTransactionAsync(async _ =>
            {
                await _repository.DeleteAsync(c, ct);
            }, ct);
            return true;
        }
    }
}
