using System;

namespace BitsBlog.Application.DTOs
{
    public record CommentDto(int Id, int PostId, string Content, DateTime Created);
}
