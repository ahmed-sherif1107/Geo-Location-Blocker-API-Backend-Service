using System.Collections.Concurrent;
using Countries.Models;
using Countries.Dtos;

namespace Countries.Services
{
    public class BlockedAttemptsRepository : IBlockedAttemptsRepository
    {
        private readonly ConcurrentDictionary<Guid, BlockedAttemptLog> _attempts = new();

        public async Task AddAttemptAsync(BlockedAttemptLog attempt)
        {
            _attempts.TryAdd(attempt.Id, attempt);
            await Task.CompletedTask;
        }

        public async Task<PaginatedResponse<BlockedAttemptLog>> GetBlockedAttemptsAsync(PaginationRequest request)
        {
            var query = _attempts.Values.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(a =>
                    a.CountryCode.ToLower().Contains(searchTerm) ||
                    a.CountryName.ToLower().Contains(searchTerm) ||
                    a.IPAddress.Contains(searchTerm));
            }

            var totalCount = query.Count();
            var items = query
                .OrderByDescending(a => a.Timestamp)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return await Task.FromResult(new PaginatedResponse<BlockedAttemptLog>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.Page,
                PageSize = request.PageSize
            });
        }

        public async Task ClearOldAttemptsAsync(DateTime before)
        {
            var attemptsToRemove = _attempts
                .Where(kvp => kvp.Value.Timestamp < before)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var id in attemptsToRemove)
            {
                _attempts.TryRemove(id, out _);
            }

            await Task.CompletedTask;
        }
    }
}