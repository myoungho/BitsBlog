using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace BitsBlog.Infrastructure
{
    public class BitsBlogDbContextFactory : IDesignTimeDbContextFactory<BitsBlogDbContext>
    {
        public BitsBlogDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            var options = new DbContextOptionsBuilder<BitsBlogDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new BitsBlogDbContext(options);
        }
    }
}
