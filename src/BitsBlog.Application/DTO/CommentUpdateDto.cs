namespace BitsBlog.Application.DTO
{
    public class CommentUpdateDto
    {
        public int PostId { get; set; }
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
