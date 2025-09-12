namespace BitsBlog.Application.DTO
{
    public class PostUpdateDto
    {
        /// <summary>ID of the post to update</summary>
        public int Id { get; set; }

        /// <summary>Updated title</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Updated body (HTML allowed; sanitized on server)</summary>
        public string Content { get; set; } = string.Empty;
    }
}

