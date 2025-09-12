using System.Collections.Generic;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;

namespace BitsBlog.Application.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetCommentsByPostIdAsync(int postId);
        Task<IReadOnlyList<CommentDto>> GetCommentsByPostIdPagedAsync(int postId, int skip, int take, string? q = null, string? sort = null);
        Task<int> CountByPostIdAsync(int postId, string? q = null);
        Task<CommentDto> CreateAsync(int postId, string content, string? authorLoginId = null, string? authorDisplayName = null, int? customerId = null);
        Task<CommentDto?> GetByIdAsync(int id);
        Task<CommentDto?> UpdateAsync(int id, string content);
        Task<bool> DeleteAsync(int id);
    }
}
