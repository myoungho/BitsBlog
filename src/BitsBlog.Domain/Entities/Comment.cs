using System;

namespace BitsBlog.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string? AuthorLoginId { get; set; }
        public string? AuthorDisplayName { get; set; }
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public Post Post { get; set; } = null!;
    }
}
