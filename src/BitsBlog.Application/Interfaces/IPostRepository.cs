using System.Collections.Generic;
using System.Threading.Tasks;
using BitsBlog.Domain.Entities;

namespace BitsBlog.Application.Interfaces
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(int id);
        Task<Post> AddAsync(Post post);
    }
}
