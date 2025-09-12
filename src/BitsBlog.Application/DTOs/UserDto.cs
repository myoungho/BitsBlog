using System;

namespace BitsBlog.Application.DTOs
{
    public class UserDto : IEquatable<UserDto>
    {
        public int Id { get; }
        public string LoginId { get; }
        public string DisplayName { get; }
        public string Role { get; }
        public DateTime Created { get; }

        public UserDto(int id, string loginId, string displayName, string role, DateTime created)
        {
            Id = id;
            LoginId = loginId;
            DisplayName = displayName;
            Role = role;
            Created = created;
        }

        public override bool Equals(object? obj) => Equals(obj as UserDto);
        public bool Equals(UserDto? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && LoginId == other.LoginId && DisplayName == other.DisplayName && Role == other.Role && Created == other.Created;
        }
        public override int GetHashCode() => HashCode.Combine(Id, LoginId, DisplayName, Role, Created);
    }
}

