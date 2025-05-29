using Countries.Models;
using Countries.Dtos;

namespace Countries.Services
{
    public interface IBlockedAttemptsRepository
    {
        Task AddAttemptAsync(BlockedAttemptLog attempt);
        Task<PaginatedResponse<BlockedAttemptLog>> GetBlockedAttemptsAsync(PaginationRequest request);
        Task ClearOldAttemptsAsync(DateTime before);
    }
}