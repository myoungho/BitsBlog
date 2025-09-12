using System.ComponentModel.DataAnnotations;

namespace BitsBlog.Web.Models
{
    public class EditPostViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a title.")]
        [StringLength(200, ErrorMessage = "The title must be {1} characters or fewer.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter content.")]
        [StringLength(4000, ErrorMessage = "Content must be {1} characters or fewer.")]
        public string Content { get; set; } = string.Empty;
    }
}

