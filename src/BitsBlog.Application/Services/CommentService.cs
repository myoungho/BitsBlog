using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;
using BitsBlog.Application.Interfaces;
using BitsBlog.Domain.Entities;

namespace BitsBlog.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly IRepository<Comment> _repository;
        public CommentService(IRepository<Comment> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByPostIdAsync(int postId)
        {
            var comments = await _repository.GetAllAsync();
            comments = comments.Where(c => c.PostId == postId);
            return comments.Select(c => new CommentDto(c.Id, c.PostId, c.Content, c.Created));
        }

        public async Task<CommentDto> CreateAsync(int postId, string content)
        {
            var comment = new Comment { PostId = postId, Content = content };
            var created = await _repository.InsertAsync(comment);
            return new CommentDto(created.Id, created.PostId, created.Content, created.Created);
        }
    }
}
