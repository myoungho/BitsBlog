using System;

namespace BitsBlog.Application.DTO
{
    public class UserDto 
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
    }
}
