using BitsBlog.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BitsBlog.Web.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_RedirectsToPostsIndex()
        {
            var controller = new HomeController();

            var result = controller.Index();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Posts", redirect.ControllerName);
        }
    }
}
