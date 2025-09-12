using System;

namespace BitsBlog.Application.DTOs
{
    public class ProfileDto
    {
        public string LoginId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public DateTime Created { get; set; }
    }
}

