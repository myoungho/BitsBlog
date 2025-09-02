using System;

namespace BitsBlog.Application.DTOs
{
    public class CommentDto : IEquatable<CommentDto>
    {
        public int Id { get; }
        public int PostId { get; }
        public string Content { get; }
        public DateTime Created { get; }

        public CommentDto(int id, int postId, string content, DateTime created)
        {
            Id = id;
            PostId = postId;
            Content = content;
            Created = created;
        }

        public override bool Equals(object? obj) => Equals(obj as CommentDto);

        public bool Equals(CommentDto? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && PostId == other.PostId && Content == other.Content && Created == other.Created;
        }

        public override int GetHashCode() => HashCode.Combine(Id, PostId, Content, Created);
    }
}
