using System.Collections.Generic;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;

namespace BitsBlog.Application.Services
{
    public interface IPostService
    {
        Task<IEnumerable<PostDto>> GetPostsAsync(System.Threading.CancellationToken ct = default);
        Task<IReadOnlyList<PostDto>> GetPagedAsync(BitsBlog.Application.DTOs.PostQueryDto query, System.Threading.CancellationToken ct = default);
        Task<int> CountAsync(BitsBlog.Application.DTOs.PostQueryDto query, System.Threading.CancellationToken ct = default);
        Task<PostDto> CreateAsync(BitsBlog.Application.DTOs.PostCreateDto dto, System.Threading.CancellationToken ct = default);
        Task<PostDto?> GetByIdAsync(int id, System.Threading.CancellationToken ct = default);
        Task<PostDto?> UpdateAsync(BitsBlog.Application.DTOs.PostUpdateDto dto, System.Threading.CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, System.Threading.CancellationToken ct = default);
    }
}
