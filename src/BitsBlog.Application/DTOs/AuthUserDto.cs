namespace BitsBlog.Application.DTOs
{
    public class AuthUserDto
    {
        public int? Id { get; set; }
        public string LoginId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }
}

