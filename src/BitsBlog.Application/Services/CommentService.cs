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

        public async Task<BitsBlog.Application.DTO.Common.PagedResult<CommentDto>> GetPagedAsync(BitsBlog.Application.DTO.CommentQueryDto queryDto, System.Threading.CancellationToken ct = default)
        {
            var q = _repository.AsNoTracking().Where(c => c.PostId == queryDto.PostId);
            // No search; simple paging only

            var projected = q.Select(c => new CommentDto(c.Id, c.PostId, c.Content, c.Created)
            {
                AuthorLoginId = c.AuthorLoginId,
                AuthorDisplayName = c.AuthorDisplayName,
                CustomerId = c.CustomerId
            });

            string defaultSort = nameof(CommentDto.Created);
            string? sortBy = defaultSort;
            string? sortOrder = "desc";

            var paged = await _repository.PagedAsync<Comment, CommentDto>(
                projected,
                queryDto.Page,
                queryDto.PageSize);

            return paged;
        }

        public Task<int> CountAsync(BitsBlog.Application.DTO.CommentQueryDto queryDto, System.Threading.CancellationToken ct = default)
        {
            var query = _repository.AsNoTracking().Where(c => c.PostId == queryDto.PostId);
            if (!string.IsNullOrWhiteSpace(queryDto.Q))
            {
                var term = queryDto.Q.Trim();
                query = query.Where(c => EF.Functions.Like(c.Content, "%" + term + "%") || EF.Functions.Like(c.AuthorDisplayName!, "%" + term + "%"));
            }
            return query.CountAsync(ct);
        }

        public async Task<CommentDto> CreateAsync(BitsBlog.Application.DTO.CommentCreateDto dto, System.Threading.CancellationToken ct = default)
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

        public async Task<CommentDto?> UpdateAsync(BitsBlog.Application.DTO.CommentUpdateDto dto, System.Threading.CancellationToken ct = default)
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
