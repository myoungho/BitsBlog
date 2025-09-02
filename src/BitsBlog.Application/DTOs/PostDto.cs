using System;

namespace BitsBlog.Application.DTOs
{
    public class PostDto : IEquatable<PostDto>
    {
        public int Id { get; }
        public string Title { get; }
        public string Content { get; }
        public DateTime Created { get; }

        public PostDto(int id, string title, string content, DateTime created)
        {
            Id = id;
            Title = title;
            Content = content;
            Created = created;
        }

        public override bool Equals(object? obj) => Equals(obj as PostDto);

        public bool Equals(PostDto? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Title == other.Title && Content == other.Content && Created == other.Created;
        }

        public override int GetHashCode() => HashCode.Combine(Id, Title, Content, Created);
    }
}
