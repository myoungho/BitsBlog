using System.Threading;
using System.Threading.Tasks;

namespace BitsBlog.Application.Services
{
    public interface IAdminMaintenanceService
    {
        Task<bool> DeleteUserAsync(int userId, string mode = "anonymize", CancellationToken ct = default);
    }
}

