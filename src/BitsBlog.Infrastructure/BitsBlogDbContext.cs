using Microsoft.EntityFrameworkCore;
using BitsBlog.Domain.Entities;

namespace BitsBlog.Infrastructure
{
    public class BitsBlogDbContext : DbContext
    {
        public BitsBlogDbContext(DbContextOptions<BitsBlogDbContext> options) : base(options) { }

        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
    }
}
