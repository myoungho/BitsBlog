using Microsoft.EntityFrameworkCore;
using BitsBlog.Domain.Entities;

namespace BitsBlog.Infrastructure
{
    public class BitsBlogDbContext : DbContext
    {
        public BitsBlogDbContext(DbContextOptions<BitsBlogDbContext> options) : base(options) { }

        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Customer> Customers => Set<Customer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Customer>(e =>
            {
                e.HasIndex(x => x.LoginId).IsUnique();
                e.Property(x => x.LoginId).HasMaxLength(256).IsRequired();
                e.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
                e.Property(x => x.Role).HasMaxLength(20).IsRequired();
            });
        }
    }
}
