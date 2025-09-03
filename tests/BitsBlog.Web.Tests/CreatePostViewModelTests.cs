using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BitsBlog.Web.Models;
using Xunit;

namespace BitsBlog.Web.Tests
{
    public class CreatePostViewModelTests
    {
        private static IList<ValidationResult> ValidateModel(object model)
        {
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void Validation_Fails_When_Title_Or_Content_Empty()
        {
            var model = new CreatePostViewModel { Title = "", Content = "" };

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreatePostViewModel.Title)));
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreatePostViewModel.Content)));
        }

        [Fact]
        public void Validation_Fails_When_OverMaxLength()
        {
            var longTitle = new string('a', 201);
            var longContent = new string('b', 4001);
            var model = new CreatePostViewModel { Title = longTitle, Content = longContent };

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreatePostViewModel.Title)));
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreatePostViewModel.Content)));
        }

        [Fact]
        public void Validation_Succeeds_When_WithinConstraints()
        {
            var model = new CreatePostViewModel { Title = "ok", Content = "valid" };

            var results = ValidateModel(model);

            Assert.Empty(results);
        }
    }
}

