using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BitsBlog.Application.DTO;
using BitsBlog.Application.Interfaces;
using BitsBlog.Application.Services;
using BitsBlog.Domain.Entities;
using Moq;
using Xunit;

namespace BitsBlog.WebApi.Tests
{
    public class PostServiceTests
    {
        [Fact]
        public async Task GetPagedAsync_Filters_By_Title_When_Q_Provided()
        {
            var data = new List<Post>
            {
                new Post { Id = 1, Title = "Hello World", Content = "x" },
                new Post { Id = 2, Title = "Another", Content = "hello body" },
                new Post { Id = 3, Title = "HELLO Dotnet", Content = "y" },
            };

            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.AsNoTracking()).Returns(data.AsQueryable());
            repo.Setup(r => r.ToListAsync(It.IsAny<IQueryable<PostDto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<PostDto> q, CancellationToken _) => q.ToList());
            repo.Setup(r => r.CountAsync(It.IsAny<IQueryable<PostDto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IQueryable<PostDto> q, CancellationToken _) => q.Count());

            var service = new PostService(repo.Object);
            var paged = await service.GetPagedAsync(new PostQueryDto { Q = "hello", Page = 1, PageSize = 10 });

            Assert.Equal(2, paged.Total);
            Assert.All(paged.Items, p => Assert.Contains("hello", p.Title.ToLowerInvariant()));
        }
    }
}

