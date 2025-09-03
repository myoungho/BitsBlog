using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;
using BitsBlog.Application.Interfaces;
using BitsBlog.Application.Services;
using BitsBlog.Domain.Entities;
using BitsBlog.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BitsBlog.WebApi.Tests
{
    public class PostsControllerTests
    {
        [Fact]
        public async Task Get_ReturnsPostsFromService()
        {
            var posts = new[]
            {
                new Post { Id = 1, Title = "Title1", Content = "Content1", Created = DateTime.UtcNow },
                new Post { Id = 2, Title = "Title2", Content = "Content2", Created = DateTime.UtcNow }
            };
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.GetAllAsync()).ReturnsAsync(posts);
            var service = new PostService(repo.Object);
            var controller = new PostsController(service, new Ganss.Xss.HtmlSanitizer());

            var result = await controller.Get();

            Assert.Equal(posts.Select(p => new PostDto(p.Id, p.Title, p.Content, p.Created)), result);
        }

        [Fact]
        public async Task Post_ReturnsCreatedPost()
        {
            var post = new Post { Id = 1, Title = "New", Content = "Body", Created = DateTime.UtcNow };
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.InsertAsync(It.IsAny<Post>())).ReturnsAsync(post);
            var service = new PostService(repo.Object);
            var controller = new PostsController(service, new Ganss.Xss.HtmlSanitizer());
            var request = new PostsController.CreatePostRequest(post.Title, post.Content);

            var response = await controller.Post(request);

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
            repo.Setup(r => r.GetByIdAsync(post.Id)).ReturnsAsync(post);
            var service = new PostService(repo.Object);
            var controller = new PostsController(service, new Ganss.Xss.HtmlSanitizer());

            var result = await controller.GetById(post.Id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<PostDto>(ok.Value);
            Assert.Equal(post.Id, dto.Id);
            Assert.Equal(post.Title, dto.Title);
            Assert.Equal(post.Content, dto.Content);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Post)null!);
            var service = new PostService(repo.Object);
            var controller = new PostsController(service, new Ganss.Xss.HtmlSanitizer());

            var result = await controller.GetById(123);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Put_UpdatesAndReturnsOk_WhenFound()
        {
            var post = new Post { Id = 7, Title = "Old", Content = "OldC", Created = DateTime.UtcNow };
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.GetByIdAsync(post.Id)).ReturnsAsync(post);
            repo.Setup(r => r.UpdateAsync(post)).Returns(Task.CompletedTask);
            repo.Setup(r => r.SaveDbContextChangesAsync()).Returns(Task.CompletedTask);
            var service = new PostService(repo.Object);
            var controller = new PostsController(service, new Ganss.Xss.HtmlSanitizer());

            var res = await controller.Put(post.Id, new PostsController.UpdatePostRequest("New", "NewC"));

            var ok = Assert.IsType<OkObjectResult>(res.Result);
            var dto = Assert.IsType<PostDto>(ok.Value);
            Assert.Equal(post.Id, dto.Id);
            Assert.Equal("New", dto.Title);
            Assert.Equal("NewC", dto.Content);
        }

        [Fact]
        public async Task Put_ReturnsNotFound_WhenMissing()
        {
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Post)null!);
            var service = new PostService(repo.Object);
            var controller = new PostsController(service, new Ganss.Xss.HtmlSanitizer());

            var res = await controller.Put(999, new PostsController.UpdatePostRequest("A", "B"));

            Assert.IsType<NotFoundResult>(res.Result);
        }

        [Fact]
        public async Task Delete_NoContent_WhenFound()
        {
            var post = new Post { Id = 11, Title = "t", Content = "c", Created = DateTime.UtcNow };
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.GetByIdAsync(post.Id)).ReturnsAsync(post);
            repo.Setup(r => r.DeleteAsync(post)).Returns(Task.CompletedTask);
            repo.Setup(r => r.SaveDbContextChangesAsync()).Returns(Task.CompletedTask);
            var service = new PostService(repo.Object);
            var controller = new PostsController(service, new Ganss.Xss.HtmlSanitizer());

            var result = await controller.Delete(post.Id);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_NotFound_WhenMissing()
        {
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Post)null!);
            var service = new PostService(repo.Object);
            var controller = new PostsController(service, new Ganss.Xss.HtmlSanitizer());

            var result = await controller.Delete(123);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
