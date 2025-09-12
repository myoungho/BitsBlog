using System.Collections.Generic;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;

namespace BitsBlog.Application.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetCommentsByPostIdAsync(int postId);
        Task<IReadOnlyList<CommentDto>> GetPagedAsync(BitsBlog.Application.DTOs.CommentQueryDto query);
        Task<int> CountAsync(BitsBlog.Application.DTOs.CommentQueryDto query);
        Task<CommentDto> CreateAsync(BitsBlog.Application.DTOs.CommentCreateDto dto);
        Task<CommentDto?> GetByIdAsync(int id);
        Task<CommentDto?> UpdateAsync(BitsBlog.Application.DTOs.CommentUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
