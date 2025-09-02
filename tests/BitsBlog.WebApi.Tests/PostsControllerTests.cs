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
            var controller = new PostsController(service);

            var result = await controller.Get();

            Assert.Equal(posts.Select(p => new PostDto(p.Id, p.Title, p.Content, p.Created)), result);
        }

        [Fact]
        public async Task Post_ReturnsCreatedPost()
        {
            var post = new Post { Id = 1, Title = "New", Content = "Body", Created = DateTime.UtcNow };
            var repo = new Mock<IRepository<Post>>();
            repo.Setup(r => r.AddAsync(It.IsAny<Post>())).ReturnsAsync(post);
            var service = new PostService(repo.Object);
            var controller = new PostsController(service);
            var request = new PostsController.CreatePostRequest(post.Title, post.Content);

            var response = await controller.Post(request);

            var created = Assert.IsType<CreatedAtActionResult>(response.Result);
            var dto = Assert.IsType<PostDto>(created.Value);
            Assert.Equal(post.Id, dto.Id);
            Assert.Equal(post.Title, dto.Title);
            Assert.Equal(post.Content, dto.Content);
        }
    }
}
