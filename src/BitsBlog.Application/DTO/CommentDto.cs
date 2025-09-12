using System;

namespace BitsBlog.Application.DTO
{
    public class CommentDto
    {
        public int Id { get; }
        public int PostId { get; }
        public string Content { get; }
        public DateTime Created { get; }
        public string? AuthorLoginId { get; init; }
        public string? AuthorDisplayName { get; init; }
        public int? CustomerId { get; init; }

        public CommentDto(int id, int postId, string content, DateTime created)
        {
            Id = id;
            PostId = postId;
            Content = content;
            Created = created;
        }
    }
}
