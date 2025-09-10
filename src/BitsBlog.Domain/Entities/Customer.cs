using System;

namespace BitsBlog.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        // LoginId is Email (unique)
        public string LoginId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        // Roles: "User" or "Admin"
        public string Role { get; set; } = "User";
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}

