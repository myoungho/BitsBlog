using System;
using System.Collections.Generic;
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
    public class CommentsControllerTests
    {
        private CommentsController CreateControllerWithUser(ICommentService service, string role = "User", string loginId = "user1")
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, loginId),
            }, "mock"));

            var controller = new CommentsController(service, new Ganss.Xss.HtmlSanitizer())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
            return controller;
        }

        [Fact]
        public async Task Get_ReturnsOk_WithPagedResult_WhenNoPostId()
        {
            var paged = new BitsBlog.Application.DTO.Common.PagedResult<CommentDto>
            {
                Items = new List<CommentDto> { new CommentDto(1, 10, "c", DateTime.UtcNow) },
                Total = 1,
                Page = 1,
                PageSize = 10
            };
            var service = new Mock<ICommentService>();
            service.Setup(s => s.GetPagedAsync(It.IsAny<CommentQueryDto>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(paged);

            var controller = new CommentsController(service.Object, new Ganss.Xss.HtmlSanitizer())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            var res = await controller.Get(new CommentQueryDto { Page = 1, PageSize = 10 });

            var ok = Assert.IsType<OkObjectResult>(res);
            var body = Assert.IsType<BitsBlog.Application.DTO.Common.PagedResult<CommentDto>>(ok.Value);
            Assert.Equal(1, body.Total);
        }

        [Fact]
        public async Task Delete_AsAdmin_AllowsDeletingAnyComment()
        {
            var repo = new Mock<IRepository<Comment>>();
            var existing = new Comment { Id = 5, PostId = 99, Content = "x", AuthorLoginId = "someone" };
            repo.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
            repo.Setup(r => r.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
                .Callback(async (Func<CancellationToken, Task> action, CancellationToken token) => await action(token));

            var deleted = false;
            repo.Setup(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()))
                .Callback(() => deleted = true)
                .Returns(Task.CompletedTask);

            var service = new CommentService(repo.Object);
            var controller = CreateControllerWithUser(service, role: "Admin", loginId: "admin");

            var result = await controller.Delete(existing.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.True(deleted);
        }

        [Fact]
        public async Task Delete_AsNonAuthorUser_Forbid()
        {
            var repo = new Mock<IRepository<Comment>>();
            var existing = new Comment { Id = 7, PostId = 2, Content = "y", AuthorLoginId = "other" };
            repo.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
            var service = new CommentService(repo.Object);
            var controller = CreateControllerWithUser(service, role: "User", loginId: "not-author");

            var result = await controller.Delete(existing.Id);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Put_AsAuthor_UpdatesAndReturnsOk()
        {
            var repo = new Mock<IRepository<Comment>>();
            var existing = new Comment { Id = 9, PostId = 1, Content = "old", AuthorLoginId = "author1" };
            repo.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
            repo.Setup(r => r.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
                .Callback(async (Func<CancellationToken, Task> action, CancellationToken token) => await action(token));

            var service = new CommentService(repo.Object);
            var controller = CreateControllerWithUser(service, role: "User", loginId: "author1");

            var res = await controller.Put(new CommentUpdateDto { PostId = existing.PostId, CommentId = existing.Id, Content = "new" });

            var ok = Assert.IsType<OkObjectResult>(res);
            var dto = Assert.IsType<CommentDto>(ok.Value);
            Assert.Equal("new", dto.Content);
        }
    }
}
