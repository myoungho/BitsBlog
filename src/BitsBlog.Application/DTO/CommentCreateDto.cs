namespace BitsBlog.Application.DTO
{
    public class CommentCreateDto
    {
        public int PostId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? AuthorLoginId { get; set; }
        public string? AuthorDisplayName { get; set; }
        public int? CustomerId { get; set; }
    }
}

