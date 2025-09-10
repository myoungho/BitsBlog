using System.Collections.Generic;
using BitsBlog.Application.DTOs;

namespace BitsBlog.Web.Models
{
    public class PostDetailsViewModel
    {
        public PostDto Post { get; set; } = null!;
        public List<CommentDto> Comments { get; set; } = new();
    }
}

