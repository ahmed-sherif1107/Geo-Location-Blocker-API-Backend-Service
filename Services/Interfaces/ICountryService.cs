using Countries.Models;

namespace Countries.Services.Interfaces
{
	 public interface ICountryService
    {
        Task<CountryInfo?> GetCountryByCodeAsync(string countryCode);
        bool IsValidCountryCode(string countryCode);
    }
}
