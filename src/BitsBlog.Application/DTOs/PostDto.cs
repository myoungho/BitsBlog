using System;

namespace BitsBlog.Application.DTOs
{
    public record PostDto(int Id, string Title, string Content, DateTime Created);
}
