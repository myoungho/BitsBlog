using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BitsBlog.Application.DTO;
using BitsBlog.Application.Interfaces;
using BitsBlog.Application.Services;
using BitsBlog.Domain.Entities;
using BitsBlog.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BitsBlog.WebApi.Tests
{
    public class PostsControllerTests
    {
        private PostsController CreateControllerWithUser(IPostService service, string role = "User", string loginId = "test-user")
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, loginId),
            }, "mock"));

            var controller = new PostsController(service, new Ganss.Xss.HtmlSanitizer())
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = user }
                }
            };
            return controller;
        }

        [Fact]
        public async Task Get_ReturnsPostsFromService()
        {
            var posts = new List<PostDto>
            {
                new PostDto(1, "Title1", "Content1", DateTime.UtcNow),
                new PostDto(2, "Title2", "Content2", DateTime.UtcNow)
            };

            var serviceMock = new Mock<IPostService>();
            serviceMock.Setup(s => s.GetPagedAsync(It.IsAny<PostQueryDto>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(posts);
            serviceMock.Setup(s => s.CountAsync(It.IsAny<PostQueryDto>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(posts.Count);

            var controller = new PostsController(serviceMock.Object, new Ganss.Xss.HtmlSanitizer())
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var result = await controller.Get(new PostQueryDto());

            Assert.Equal(posts.Count, result.Count());
        }

        [Fact]
        public async Task Post_ReturnsCreatedPost()
        {
            var post = new Post { Id = 1, Title = "New", Content = "Body", Created = DateTime.UtcNow, AuthorLoginId = "test-user" };
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.InsertAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>())).ReturnsAsync(post);
            repo.Setup(r => r.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
                .Callback(async (Func<CancellationToken, Task> action, CancellationToken token) => await action(token));

            var service = new PostService(repo.Object);
            var controller = CreateControllerWithUser(service);
            
            var response = await controller.Post(new PostCreateDto { Title = post.Title, Content = post.Content });

            var created = Assert.IsType<CreatedAtActionResult>(response.Result);
            var dto = Assert.IsType<PostDto>(created.Value);
            Assert.Equal(post.Id, dto.Id);
            Assert.Equal(post.Title, dto.Title);
            Assert.Equal(post.Content, dto.Content);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            var post = new Post { Id = 5, Title = "T", Content = "C", Created = DateTime.UtcNow };
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.GetByIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);
            var service = new PostService(repo.Object);
            var controller = new PostsController(service, new Ganss.Xss.HtmlSanitizer());

            var result = await controller.GetById(post.Id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<PostDto>(ok.Value);
            Assert.Equal(post.Id, dto.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((Post)null!);
            var service = new PostService(repo.Object);
            var controller = new PostsController(service, new Ganss.Xss.HtmlSanitizer());

            var result = await controller.GetById(123);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Put_UpdatesAndReturnsOk_WhenFound()
        {
            var post = new Post { Id = 7, Title = "Old", Content = "OldC", Created = DateTime.UtcNow, AuthorLoginId = "test-user" };
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.GetByIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);
            repo.Setup(r => r.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
                .Callback(async (Func<CancellationToken, Task> action, CancellationToken token) => await action(token));
            
            var service = new PostService(repo.Object);
            var controller = CreateControllerWithUser(service);

            var res = await controller.Put(post.Id, new PostUpdateDto { Id = post.Id, Title = "New", Content = "NewC" });

            var ok = Assert.IsType<OkObjectResult>(res.Result);
            var dto = Assert.IsType<PostDto>(ok.Value);
            Assert.Equal("New", dto.Title);
        }

        [Fact]
        public async Task Put_ReturnsNotFound_WhenMissing()
        {
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((Post)null!);
            var service = new PostService(repo.Object);
            var controller = CreateControllerWithUser(service);

            var res = await controller.Put(999, new PostUpdateDto { Id = 999, Title = "A", Content = "B" });

            Assert.IsType<NotFoundResult>(res.Result);
        }

        [Fact]
        public async Task Delete_NoContent_WhenFound()
        {
            var post = new Post { Id = 11, Title = "t", Content = "c", Created = DateTime.UtcNow, AuthorLoginId = "test-user" };
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.GetByIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);
            repo.Setup(r => r.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
                .Callback(async (Func<CancellationToken, Task> action, CancellationToken token) => await action(token));
            
            var service = new PostService(repo.Object);
            var controller = CreateControllerWithUser(service);

            var result = await controller.Delete(post.Id);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_NotFound_WhenMissing()
        {
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((Post)null!);
            var service = new PostService(repo.Object);
            var controller = CreateControllerWithUser(service);

            var result = await controller.Delete(123);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}