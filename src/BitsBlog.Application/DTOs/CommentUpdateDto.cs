namespace BitsBlog.Application.DTOs
{
    public class CommentUpdateDto
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}

