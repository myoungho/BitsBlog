using System.Collections.Generic;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;

namespace BitsBlog.Application.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetCommentsByPostIdAsync(int postId);
        Task<CommentDto> CreateAsync(int postId, string content);
    }
}
