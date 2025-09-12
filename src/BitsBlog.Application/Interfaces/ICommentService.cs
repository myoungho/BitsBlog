using System.Collections.Generic;
using System.Threading.Tasks;
using BitsBlog.Application.DTO;
using BitsBlog.Application.DTO.Common;

namespace BitsBlog.Application.Interfaces
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetCommentsByPostIdAsync(int postId, System.Threading.CancellationToken ct = default);
        Task<PagedResult<CommentDto>> GetPagedAsync(CommentQueryDto query, System.Threading.CancellationToken ct = default);
        Task<int> CountAsync(CommentQueryDto query, System.Threading.CancellationToken ct = default);
        Task<CommentDto> CreateAsync(CommentCreateDto dto, System.Threading.CancellationToken ct = default);
        Task<CommentDto?> GetByIdAsync(int id, System.Threading.CancellationToken ct = default);
        Task<CommentDto?> UpdateAsync(CommentUpdateDto dto, System.Threading.CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, System.Threading.CancellationToken ct = default);
    }
}
