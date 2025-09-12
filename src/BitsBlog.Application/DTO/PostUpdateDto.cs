namespace BitsBlog.Application.DTO
{
    public class PostUpdateDto
    {
        /// <summary>수정할 게시글 ID</summary>
        public int Id { get; set; }
        /// <summary>수정할 제목</summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>수정할 본문(HTML 가능, 서버에서 sanitize)</summary>
        public string Content { get; set; } = string.Empty;
    }
}
