namespace BitsBlog.Application.DTO
{
    public class ChangePasswordDto
    {
        public string LoginId { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}

