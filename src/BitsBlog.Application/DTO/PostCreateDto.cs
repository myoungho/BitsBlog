namespace BitsBlog.Application.DTO
{
    public class PostCreateDto
    {
        /// <summary>Post title</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Post body (HTML allowed; sanitized on server)</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>Author login ID (injected from JWT on server)</summary>
        public string? AuthorLoginId { get; set; }

        /// <summary>Author display name (injected from JWT on server)</summary>
        public string? AuthorDisplayName { get; set; }

        /// <summary>Author customer ID (injected from JWT on server)</summary>
        public int? CustomerId { get; set; }
    }
}

