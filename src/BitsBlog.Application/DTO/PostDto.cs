using System;

namespace BitsBlog.Application.DTO
{
    public class PostDto
    {
        public int Id { get; }
        public string Title { get; }
        public string Content { get; }
        public DateTime Created { get; }
        public string? AuthorLoginId { get; init; }
        public string? AuthorDisplayName { get; init; }
        public int? CustomerId { get; init; }

        public PostDto(int id, string title, string content, DateTime created)
        {
            Id = id;
            Title = title;
            Content = content;
            Created = created;
        }
    }
}
