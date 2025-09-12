using System.Collections.Generic;
using System.Threading.Tasks;
using BitsBlog.Application.DTO;

namespace BitsBlog.Application.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<PostDto>> GetPostsAsync(System.Threading.CancellationToken ct = default);
        Task<IReadOnlyList<PostDto>> GetPagedAsync(PostQueryDto query, System.Threading.CancellationToken ct = default);
        Task<int> CountAsync(PostQueryDto query, System.Threading.CancellationToken ct = default);
        Task<PostDto> CreateAsync(PostCreateDto dto, System.Threading.CancellationToken ct = default);
        Task<PostDto?> GetByIdAsync(int id, System.Threading.CancellationToken ct = default);
        Task<PostDto?> UpdateAsync(PostUpdateDto dto, System.Threading.CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, System.Threading.CancellationToken ct = default);
    }
}