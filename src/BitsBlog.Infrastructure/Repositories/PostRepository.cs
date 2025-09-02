using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitsBlog.Application.Interfaces;
using BitsBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BitsBlog.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly BitsBlogDbContext _context;
        public PostRepository(BitsBlogDbContext context)
        {
            _context = context;
        }

        public async Task<Post> AddAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await _context.Posts.OrderByDescending(p => p.Created).ToListAsync();
        }

        public async Task<Post?> GetByIdAsync(int id)
        {
            return await _context.Posts.FindAsync(id);
        }
    }
}
