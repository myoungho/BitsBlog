using System;
using System.Collections.Generic;

namespace BitsBlog.Domain.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string? AuthorLoginId { get; set; }
        public string? AuthorDisplayName { get; set; }
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
