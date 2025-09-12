using System.Collections.Generic;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;

namespace BitsBlog.Application.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetCommentsByPostIdAsync(int postId, System.Threading.CancellationToken ct = default);
        Task<IReadOnlyList<CommentDto>> GetPagedAsync(BitsBlog.Application.DTOs.CommentQueryDto query, System.Threading.CancellationToken ct = default);
        Task<int> CountAsync(BitsBlog.Application.DTOs.CommentQueryDto query, System.Threading.CancellationToken ct = default);
        Task<CommentDto> CreateAsync(BitsBlog.Application.DTOs.CommentCreateDto dto, System.Threading.CancellationToken ct = default);
        Task<CommentDto?> GetByIdAsync(int id, System.Threading.CancellationToken ct = default);
        Task<CommentDto?> UpdateAsync(BitsBlog.Application.DTOs.CommentUpdateDto dto, System.Threading.CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, System.Threading.CancellationToken ct = default);
    }
}
