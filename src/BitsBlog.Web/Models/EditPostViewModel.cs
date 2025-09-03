using System.ComponentModel.DataAnnotations;

namespace BitsBlog.Web.Models
{
    public class EditPostViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "제목을 입력하세요.")]
        [StringLength(200, ErrorMessage = "제목은 {1}자 이하여야 합니다.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "내용을 입력하세요.")]
        [StringLength(4000, ErrorMessage = "내용은 {1}자 이하여야 합니다.")]
        public string Content { get; set; } = string.Empty;
    }
}

