using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BitsBlog.Application.Interfaces;
using BitsBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BitsBlog.Application.Services
{
    public class AdminMaintenanceService : IAdminMaintenanceService
    {
        private readonly IRepository<Customer> _customers;
        private readonly IRepository<Post> _posts;
        private readonly IRepository<Comment> _comments;

        public AdminMaintenanceService(IRepository<Customer> customers, IRepository<Post> posts, IRepository<Comment> comments)
        {
            _customers = customers;
            _posts = posts;
            _comments = comments;
        }

        public async Task<bool> DeleteUserAsync(int userId, string mode = "anonymize", CancellationToken ct = default)
        {
            var user = await _customers.GetByIdAsync(userId, ct);
            if (user is null) return false;
            var loginId = user.LoginId;

            // Use a single transaction across the shared DbContext
            await _customers.ExecuteInTransactionAsync(async token =>
            {
                if (string.Equals(mode, "cascade", StringComparison.OrdinalIgnoreCase))
                {
                    // Delete user's comments
                    var delComments = await _comments.AsTracking()
                        .Where(c => c.CustomerId == userId || c.AuthorLoginId == loginId)
                        .ToListAsync(token);
                    if (delComments.Count > 0) await _comments.DeleteRangeAsync(delComments, token);

                    // Delete user's posts (which may also remove related comments by cascade or leave orphans already handled above)
                    var delPosts = await _posts.AsTracking()
                        .Where(p => p.CustomerId == userId || p.AuthorLoginId == loginId)
                        .ToListAsync(token);
                    if (delPosts.Count > 0) await _posts.DeleteRangeAsync(delPosts, token);

                    // Finally delete the user
                    await _customers.DeleteAsync(user, token);
                }
                else
                {
                    // Anonymize posts
                    var anonPosts = await _posts.AsTracking()
                        .Where(p => p.CustomerId == userId || p.AuthorLoginId == loginId)
                        .ToListAsync(token);
                    foreach (var p in anonPosts)
                    {
                        p.CustomerId = null;
                        p.AuthorLoginId = null;
                        p.AuthorDisplayName = "[deleted]";
                    }

                    // Anonymize comments
                    var anonComments = await _comments.AsTracking()
                        .Where(c => c.CustomerId == userId || c.AuthorLoginId == loginId)
                        .ToListAsync(token);
                    foreach (var c in anonComments)
                    {
                        c.CustomerId = null;
                        c.AuthorLoginId = null;
                        c.AuthorDisplayName = "[deleted]";
                    }

                    // Delete the user
                    await _customers.DeleteAsync(user, token);
                }
            }, ct);

            return true;
        }
    }
}

