using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Countries.Models;
using Microsoft.Extensions.Logging;

namespace Countries.Services
{
	public class GeoLocationService : IGeoLocationService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogger<GeoLocationService> _logger;

		public GeoLocationService(
			HttpClient httpClient,
			IConfiguration configuration,
			IHttpContextAccessor httpContextAccessor,
			ILogger<GeoLocationService> logger)
		{
			_httpClient = httpClient;
			_configuration = configuration;
			_httpContextAccessor = httpContextAccessor;
			_logger = logger;
		}

		public async Task<IPLookupResponse> GetLocationByIPAsync(string ipAddress)
		{
			try
			{
				if (!IsValidIPAddress(ipAddress))
				{
					throw new ArgumentException("Invalid IP address format", nameof(ipAddress));
				}

				var apiKey = _configuration["IPGeolocation:ApiKey"];
				if (string.IsNullOrEmpty(apiKey))
				{
					throw new InvalidOperationException("IPGeolocation API key is not configured");
				}

				_logger.LogInformation("Making request to IP Geolocation API for IP: {IPAddress}", ipAddress);

				// Format the URL with proper encoding and correct endpoint
				var requestUrl = $"v2/ipgeo?apiKey={Uri.EscapeDataString(apiKey)}&ip={Uri.EscapeDataString(ipAddress)}";
				_logger.LogDebug("Request URL: {RequestUrl}", requestUrl);

				var response = await _httpClient.GetAsync(requestUrl);

				if (!response.IsSuccessStatusCode)
				{
					var errorContent = await response.Content.ReadAsStringAsync();
					_logger.LogError("IP Geolocation API returned error: {StatusCode} - {ErrorContent}",
						response.StatusCode, errorContent);
					throw new HttpRequestException($"IP Geolocation API returned {response.StatusCode}");
				}

				var content = await response.Content.ReadAsStringAsync();
				_logger.LogDebug("Raw API Response: {Content}", content);

				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};

				var result = JsonSerializer.Deserialize<IPLookupResponse>(content, options);
				if (result == null)
				{
					throw new Exception("Failed to deserialize IP lookup response");
				}

				_logger.LogDebug("Deserialized Response: {@Result}", result);
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in GetLocationByIPAsync for IP {IPAddress}", ipAddress);
				throw;
			}
		}

		public async Task<IPLookupResponse> GetCurrentLocationAsync()
		{
			var ipAddress = GetClientIPAddress();
			return await GetLocationByIPAsync(ipAddress);
		}

		private string GetClientIPAddress()
		{
			var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

			if (string.IsNullOrEmpty(ipAddress))
			{
				throw new Exception("Could not determine client IP address");
			}

			// Handle IPv6 mapped to IPv4
			if (IPAddress.TryParse(ipAddress, out var ip))
			{
				if (ip.IsIPv4MappedToIPv6)
				{
					ipAddress = ip.MapToIPv4().ToString();
				}
			}

			return ipAddress;
		}

		private bool IsValidIPAddress(string ipAddress)
		{
			return IPAddress.TryParse(ipAddress, out _);
		}
	}
}
