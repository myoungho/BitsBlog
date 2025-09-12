namespace BitsBlog.Application.DTOs
{
    public class PostCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? AuthorLoginId { get; set; }
        public string? AuthorDisplayName { get; set; }
        public int? CustomerId { get; set; }
    }
}

