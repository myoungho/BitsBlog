namespace BitsBlog.Application.DTOs
{
    public class PostCreateDto
    {
        /// <summary>게시글 제목</summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>게시글 본문(HTML 가능, 서버에서 sanitize)</summary>
        public string Content { get; set; } = string.Empty;
        /// <summary>작성자 로그인ID(서버에서 JWT로 주입)</summary>
        public string? AuthorLoginId { get; set; }
        /// <summary>작성자 표시명(서버에서 JWT로 주입)</summary>
        public string? AuthorDisplayName { get; set; }
        /// <summary>작성자 고객ID(서버에서 JWT로 주입)</summary>
        public int? CustomerId { get; set; }
    }
}
