namespace BitsBlog.Application.DTO
{
    public class UserQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Q { get; set; }
        public string? Sort { get; set; }
    }
}

