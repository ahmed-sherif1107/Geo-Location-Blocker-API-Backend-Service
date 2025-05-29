using System.Text.Json;
using Countries.Models;
using Countries.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Countries.Services
{
	public class CountryService : ICountryService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<CountryService> _logger;
		private const string RestCountriesBaseUrl = "https://restcountries.com/v3.1/alpha/";

		public CountryService(HttpClient httpClient, ILogger<CountryService> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		public async Task<CountryInfo?> GetCountryByCodeAsync(string countryCode)
		{
			try
			{
				if (!IsValidCountryCode(countryCode))
				{
					_logger.LogWarning("Invalid country code provided: {CountryCode}", countryCode);
					return null;
				}

				var normalizedCode = countryCode.ToUpper();
				var requestUrl = $"{RestCountriesBaseUrl}{normalizedCode}";

				_logger.LogInformation("Fetching country information for code: {CountryCode}", normalizedCode);

				var response = await _httpClient.GetAsync(requestUrl);

				if (!response.IsSuccessStatusCode)
				{
					var errorContent = await response.Content.ReadAsStringAsync();
					_logger.LogError("REST Countries API returned error: {StatusCode} - {ErrorContent}",
						response.StatusCode, errorContent);
					return null;
				}

				var content = await response.Content.ReadAsStringAsync();
				_logger.LogDebug("Raw REST Countries API Response: {Content}", content);

				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};

				// The API returns an array with a single country object
				var countries = JsonSerializer.Deserialize<CountryInfo[]>(content, options);

				if (countries == null || countries.Length == 0)
				{
					_logger.LogWarning("No country information found for code: {CountryCode}", normalizedCode);
					return null;
				}

				var countryInfo = countries[0];
				_logger.LogDebug("Successfully retrieved country information: {@CountryInfo}", countryInfo);

				return countryInfo;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error fetching country information for code: {CountryCode}", countryCode);
				return null;
			}
		}

		public bool IsValidCountryCode(string countryCode)
		{
			return CountryCodes.IsValidCountryCode(countryCode);
		}
	}
}