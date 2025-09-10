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
            return comments.Select(c => new CommentDto(c.Id, c.PostId, c.Content, c.Created)
            {
                AuthorLoginId = c.AuthorLoginId,
                AuthorDisplayName = c.AuthorDisplayName
            });
        }

        public async Task<CommentDto> CreateAsync(int postId, string content, string? authorLoginId = null, string? authorDisplayName = null, int? customerId = null)
        {
            var comment = new Comment { PostId = postId, Content = content, AuthorLoginId = authorLoginId, AuthorDisplayName = authorDisplayName, CustomerId = customerId };
            var created = await _repository.InsertAsync(comment);
            await _repository.SaveDbContextChangesAsync();
            return new CommentDto(created.Id, created.PostId, created.Content, created.Created)
            {
                AuthorLoginId = created.AuthorLoginId,
                AuthorDisplayName = created.AuthorDisplayName,
                CustomerId = created.CustomerId
            };
        }

        public async Task<CommentDto?> GetByIdAsync(int id)
        {
            var c = await _repository.GetByIdAsync(id);
            if (c is null) return null;
            return new CommentDto(c.Id, c.PostId, c.Content, c.Created)
            {
                AuthorLoginId = c.AuthorLoginId,
                AuthorDisplayName = c.AuthorDisplayName,
                CustomerId = c.CustomerId
            };
        }

        public async Task<CommentDto?> UpdateAsync(int id, string content)
        {
            var c = await _repository.GetByIdAsync(id);
            if (c is null) return null;
            c.Content = content;
            await _repository.UpdateAsync(c);
            await _repository.SaveDbContextChangesAsync();
            return new CommentDto(c.Id, c.PostId, c.Content, c.Created)
            {
                AuthorLoginId = c.AuthorLoginId,
                AuthorDisplayName = c.AuthorDisplayName,
                CustomerId = c.CustomerId
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var c = await _repository.GetByIdAsync(id);
            if (c is null) return false;
            await _repository.DeleteAsync(c);
            await _repository.SaveDbContextChangesAsync();
            return true;
        }
    }
}
