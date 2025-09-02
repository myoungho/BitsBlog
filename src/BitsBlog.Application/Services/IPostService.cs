using System.Collections.Generic;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;

namespace BitsBlog.Application.Services
{
    public interface IPostService
    {
        Task<IEnumerable<PostDto>> GetPostsAsync();
        Task<PostDto> CreateAsync(string title, string content);
    }
}
