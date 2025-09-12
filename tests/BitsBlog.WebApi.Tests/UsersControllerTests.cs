using System.Threading;
using System.Threading.Tasks;
using BitsBlog.Application.Interfaces;
using BitsBlog.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BitsBlog.WebApi.Tests
{
    public class UsersControllerTests
    {
        [Fact]
        public async Task Delete_DefaultsToAnonymize_WhenModeNull()
        {
            var customers = new Mock<ICustomerService>();
            var admin = new Mock<IAdminMaintenanceService>();
            admin.Setup(a => a.DeleteUserAsync(123, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var controller = new UsersController(customers.Object, admin.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            var res = await controller.Delete(123, null);

            Assert.IsType<NoContentResult>(res);
            admin.Verify(a => a.DeleteUserAsync(123, "anonymize", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenServiceReturnsFalse()
        {
            var customers = new Mock<ICustomerService>();
            var admin = new Mock<IAdminMaintenanceService>();
            admin.Setup(a => a.DeleteUserAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var controller = new UsersController(customers.Object, admin.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            var res = await controller.Delete(999, null);

            Assert.IsType<NotFoundResult>(res);
        }
    }
}

