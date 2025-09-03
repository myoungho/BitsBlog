using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;
using BitsBlog.Web.Controllers;
using BitsBlog.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BitsBlog.Web.Tests
{
    public class PostsControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewWithPosts()
        {
            var posts = new[] { new PostDto(1, "t", "c", DateTime.UtcNow) };
            var json = JsonSerializer.Serialize(posts);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler);
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient("api")).Returns(client);
            var controller = new PostsController(factory.Object);

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<PostDto>>(view.Model);
            Assert.Equal(posts, model);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            var controller = new PostsController(new Mock<IHttpClientFactory>().Object);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithModel()
        {
            var controller = new PostsController(new Mock<IHttpClientFactory>().Object);
            controller.ModelState.AddModelError("Title", "Required");
            var model = new CreatePostViewModel { Title = "", Content = "content" };

            var result = await controller.Create(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, view.Model);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToHomeIndex()
        {
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.Created));
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/api/")
            };
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient("api")).Returns(client);
            var controller = new PostsController(factory.Object);
            var model = new CreatePostViewModel { Title = "t", Content = "c" };

            var result = await controller.Create(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        

        [Fact]
        public async Task Edit_Get_ReturnsViewWithModel()
        {
            var dto = new PostDto(10, "title", "content", DateTime.UtcNow);
            var json = JsonSerializer.Serialize(dto);
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/api/")
            };
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient("api")).Returns(client);
            var controller = new PostsController(factory.Object);

            var result = await controller.Edit(dto.Id);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EditPostViewModel>(view.Model);
            Assert.Equal(dto.Id, model.Id);
            Assert.Equal(dto.Title, model.Title);
            Assert.Equal(dto.Content, model.Content);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_RedirectsToHomeIndex()
        {
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/api/")
            };
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient("api")).Returns(client);
            var controller = new PostsController(factory.Object);
            var model = new EditPostViewModel { Id = 1, Title = "t", Content = "c" };

            var result = await controller.Edit(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public async Task Edit_Post_NotFound_ReturnsNotFound()
        {
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/api/")
            };
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient("api")).Returns(client);
            var controller = new PostsController(factory.Object);
            var model = new EditPostViewModel { Id = 999, Title = "t", Content = "c" };

            var result = await controller.Edit(model);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Post_Valid_RedirectsToHomeIndex()
        {
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NoContent));
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/api/")
            };
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient("api")).Returns(client);
            var controller = new PostsController(factory.Object);

            var result = await controller.Delete(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public async Task Delete_Post_NotFound_ReturnsNotFound()
        {
            var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/api/")
            };
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient("api")).Returns(client);
            var controller = new PostsController(factory.Object);

            var result = await controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);
        }

        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;
            public HttpRequestMessage? Request { get; private set; }

            public FakeHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Request = request;
                return Task.FromResult(_response);
            }
        }
    }
}
