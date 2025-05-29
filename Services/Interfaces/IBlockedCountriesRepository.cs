using Countries.Models;
using Countries.Dtos;

namespace Countries.Services
{
    public interface IBlockedCountriesRepository
    {
        Task<bool> AddBlockedCountryAsync(BlockedCountry country);
        Task<bool> RemoveBlockedCountryAsync(string countryCode);
        Task<BlockedCountry?> GetBlockedCountryAsync(string countryCode);
        Task<PaginatedResponse<BlockedCountry>> GetAllBlockedCountriesAsync(PaginationRequest request);
        Task<bool> IsCountryBlockedAsync(string countryCode);
        Task RemoveExpiredTemporaryBlocksAsync();
    }
}