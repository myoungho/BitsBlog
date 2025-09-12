using System.Collections.Generic;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;

namespace BitsBlog.Application.Services
{
    public interface IPostService
    {
        Task<IEnumerable<PostDto>> GetPostsAsync();
        Task<IReadOnlyList<PostDto>> GetPagedAsync(BitsBlog.Application.DTOs.PostQueryDto query);
        Task<int> CountAsync(BitsBlog.Application.DTOs.PostQueryDto query);
        Task<PostDto> CreateAsync(BitsBlog.Application.DTOs.PostCreateDto dto);
        Task<PostDto?> GetByIdAsync(int id);
        Task<PostDto?> UpdateAsync(BitsBlog.Application.DTOs.PostUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
