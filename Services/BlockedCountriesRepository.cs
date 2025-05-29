using System.Collections.Concurrent;
using Countries.Models;
using Countries.Dtos;

namespace Countries.Services
{
    public class BlockedCountriesRepository : IBlockedCountriesRepository
    {
        private readonly ConcurrentDictionary<string, BlockedCountry> _blockedCountries = new();

        public async Task<bool> AddBlockedCountryAsync(BlockedCountry country)
        {
            return await Task.FromResult(_blockedCountries.TryAdd(country.CountryCode, country));
        }

        public async Task<bool> RemoveBlockedCountryAsync(string countryCode)
        {
            return await Task.FromResult(_blockedCountries.TryRemove(countryCode, out _));
        }

        public async Task<BlockedCountry?> GetBlockedCountryAsync(string countryCode)
        {
            return await Task.FromResult(_blockedCountries.TryGetValue(countryCode, out var country) ? country : null);
        }

        public async Task<PaginatedResponse<BlockedCountry>> GetAllBlockedCountriesAsync(PaginationRequest request)
        {
            var query = _blockedCountries.Values.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.CountryCode.ToLower().Contains(searchTerm) ||
                    c.CountryName.ToLower().Contains(searchTerm));
            }

            var totalCount = query.Count();
            var items = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return await Task.FromResult(new PaginatedResponse<BlockedCountry>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.Page,
                PageSize = request.PageSize
            });
        }

        public async Task<bool> IsCountryBlockedAsync(string countryCode)
        {
            if (!_blockedCountries.TryGetValue(countryCode, out var country))
            {
                return await Task.FromResult(false);
            }

            if (country.IsTemporary && country.ExpiresAt.HasValue && country.ExpiresAt.Value < DateTime.UtcNow)
            {
                _blockedCountries.TryRemove(countryCode, out _);
                return await Task.FromResult(false);
            }

            return await Task.FromResult(true);
        }

        public async Task RemoveExpiredTemporaryBlocksAsync()
        {
            var expiredCountries = _blockedCountries
                .Where(kvp => kvp.Value.IsTemporary &&
                             kvp.Value.ExpiresAt.HasValue &&
                             kvp.Value.ExpiresAt.Value < DateTime.UtcNow)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var countryCode in expiredCountries)
            {
                _blockedCountries.TryRemove(countryCode, out _);
            }

            await Task.CompletedTask;
        }
    }
}