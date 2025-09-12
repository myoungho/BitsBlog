using System.Collections.Generic;
using System.Threading.Tasks;
using BitsBlog.Application.DTO;
using BitsBlog.Application.DTO.Common;

namespace BitsBlog.Application.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<PostDto>> GetPostsAsync(System.Threading.CancellationToken ct = default);
        Task<PagedResult<PostDto>> GetPagedAsync(PostQueryDto query, System.Threading.CancellationToken ct = default);
        Task<int> CountAsync(PostQueryDto query, System.Threading.CancellationToken ct = default);
        Task<PostDto> CreateAsync(PostCreateDto dto, System.Threading.CancellationToken ct = default);
        Task<PostDto?> GetByIdAsync(int id, System.Threading.CancellationToken ct = default);
        Task<PostDto?> UpdateAsync(PostUpdateDto dto, System.Threading.CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, System.Threading.CancellationToken ct = default);
    }
}
