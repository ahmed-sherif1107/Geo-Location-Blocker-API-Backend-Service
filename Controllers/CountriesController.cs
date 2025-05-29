using Microsoft.AspNetCore.Mvc;
using Countries.Services;
using Countries.Models;
using Countries.Dtos;
using Countries.Services.Interfaces;

namespace Countries.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CountriesController : ControllerBase
	{
		private readonly IBlockedCountriesRepository _blockedCountriesRepository;
		private readonly IBlockedAttemptsRepository _blockedAttemptsRepository;
		private readonly ICountryService _countryService;
		private readonly ILogger<CountriesController> _logger;

		public CountriesController(
			IBlockedCountriesRepository blockedCountriesRepository,
			IBlockedAttemptsRepository blockedAttemptsRepository,
			ICountryService countryService,
			ILogger<CountriesController> logger)
		{
			_blockedCountriesRepository = blockedCountriesRepository;
			_blockedAttemptsRepository = blockedAttemptsRepository;
			_countryService = countryService;
			_logger = logger;
		}

		private string GetClientIp()
		{
			// Check for X-Forwarded-For header (common when behind a proxy/load balancer)
			if (Request.Headers.ContainsKey("X-Forwarded-For"))
			{
				var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
				if (!string.IsNullOrEmpty(forwardedFor))
				{
					// Take the first IP in the chain
					return forwardedFor.Split(',')[0].Trim();
				}
			}

			// Check for X-Real-IP header
			if (Request.Headers.ContainsKey("X-Real-IP"))
			{
				var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
				if (!string.IsNullOrEmpty(realIp))
				{
					return realIp;
				}
			}

			// Fall back to connection remote IP
			return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
		}

		private async Task LogAttemptAsync(string countryCode, string countryName, string action)
		{
			try
			{
				var attempt = new BlockedAttemptLog
				{
					Id = Guid.NewGuid(),
					IPAddress = GetClientIp(),
					Timestamp = DateTime.UtcNow,
					CountryCode = countryCode,
					CountryName = countryName,
					UserAgent = Request.Headers.UserAgent.ToString(),
					BlockedStatus = true, // These are blocking operations
					RequestPath = $"{action}: {Request.Path}"
				};

				await _blockedAttemptsRepository.AddAttemptAsync(attempt);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to log blocked attempt for {Action} on {CountryCode}", action, countryCode);
			}
		}

		[HttpPost("block")]
		public async Task<IActionResult> BlockCountry([FromBody] BlockCountryRequest request)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(request.CountryCode))
				{
					return BadRequest(new { error = "Country code is required" });
				}

				// Validate country code
				if (!_countryService.IsValidCountryCode(request.CountryCode))
				{
					return BadRequest(new { error = "Invalid country code provided" });
				}

				// Fetch country information from REST Countries API
				var countryInfo = await _countryService.GetCountryByCodeAsync(request.CountryCode);
				if (countryInfo == null)
				{
					return BadRequest(new { error = "Unable to fetch country information. Please verify the country code." });
				}

				var country = new BlockedCountry
				{
					CountryCode = countryInfo.CountryCode.ToUpper(),
					CountryName = countryInfo.Name.Common,
					BlockedAt = DateTime.UtcNow,
					IsTemporary = request.IsTemporary
				};

				if (request.IsTemporary && request.DurationMinutes.HasValue)
				{
					country.ExpiresAt = DateTime.UtcNow.AddMinutes(request.DurationMinutes.Value);
				}

				var result = await _blockedCountriesRepository.AddBlockedCountryAsync(country);
				if (!result)
				{
					return Conflict(new { error = "Country is already blocked" });
				}

				// Log the blocking attempt
				await LogAttemptAsync(country.CountryCode, country.CountryName, "BLOCK_COUNTRY");

				_logger.LogInformation("Country blocked successfully: {CountryCode} - {CountryName}",
					country.CountryCode, country.CountryName);

				return Ok(country);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error blocking country {CountryCode}", request.CountryCode);
				return StatusCode(500, new { error = "An error occurred while blocking the country" });
			}
		}

		[HttpDelete("block/{countryCode}")]
		public async Task<IActionResult> UnblockCountry(string countryCode)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(countryCode))
				{
					return BadRequest(new { error = "Country code is required" });
				}

				// Get country info for logging before removing it
				var existingCountry = await _blockedCountriesRepository.GetBlockedCountryAsync(countryCode.ToUpper());

				var result = await _blockedCountriesRepository.RemoveBlockedCountryAsync(countryCode.ToUpper());
				if (!result)
				{
					return NotFound(new { error = "Country is not blocked" });
				}

				// Log the unblocking attempt
				var countryName = existingCountry?.CountryName ?? "Unknown";
				await LogAttemptAsync(countryCode.ToUpper(), countryName, "UNBLOCK_COUNTRY");

				_logger.LogInformation("Country unblocked successfully: {CountryCode}", countryCode.ToUpper());
				return NoContent();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error unblocking country {CountryCode}", countryCode);
				return StatusCode(500, new { error = "An error occurred while unblocking the country" });
			}
		}

		[HttpGet("blocked")]
		public async Task<IActionResult> GetBlockedCountries([FromQuery] PaginationRequest request)
		{
			try
			{
				var result = await _blockedCountriesRepository.GetAllBlockedCountriesAsync(request);
				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving blocked countries");
				return StatusCode(500, new { error = "An error occurred while retrieving blocked countries" });
			}
		}

		[HttpPost("temporal-block")]
		public async Task<IActionResult> TemporarilyBlockCountry([FromBody] BlockCountryRequest request)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(request.CountryCode))
				{
					return BadRequest(new { error = "Country code is required" });
				}

				if (!request.IsTemporary || !request.DurationMinutes.HasValue)
				{
					return BadRequest(new { error = "Duration is required for temporary blocks" });
				}

				// Validate country code
				if (!_countryService.IsValidCountryCode(request.CountryCode))
				{
					return BadRequest(new { error = "Invalid country code provided" });
				}

				// Fetch country information from REST Countries API
				var countryInfo = await _countryService.GetCountryByCodeAsync(request.CountryCode);
				if (countryInfo == null)
				{
					return BadRequest(new { error = "Unable to fetch country information. Please verify the country code." });
				}

				var country = new BlockedCountry
				{
					CountryCode = countryInfo.CountryCode.ToUpper(),
					CountryName = countryInfo.Name.Common,
					BlockedAt = DateTime.UtcNow,
					IsTemporary = true,
					ExpiresAt = DateTime.UtcNow.AddMinutes(request.DurationMinutes.Value)
				};

				var result = await _blockedCountriesRepository.AddBlockedCountryAsync(country);
				if (!result)
				{
					return Conflict(new { error = "Country is already blocked" });
				}

				// Log the temporary blocking attempt
				await LogAttemptAsync(country.CountryCode, country.CountryName, "TEMPORARY_BLOCK_COUNTRY");

				_logger.LogInformation("Country temporarily blocked successfully: {CountryCode} - {CountryName} for {Duration} minutes",
					country.CountryCode, country.CountryName, request.DurationMinutes.Value);

				return Ok(country);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error temporarily blocking country {CountryCode}", request.CountryCode);
				return StatusCode(500, new { error = "An error occurred while temporarily blocking the country" });
			}
		}

		[HttpGet("validate/{countryCode}")]
		public async Task<IActionResult> ValidateCountryCode(string countryCode)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(countryCode))
				{
					return BadRequest(new { error = "Country code is required" });
				}

				var isValid = _countryService.IsValidCountryCode(countryCode);
				if (!isValid)
				{
					return Ok(new { isValid = false, message = "Invalid country code" });
				}

				var countryInfo = await _countryService.GetCountryByCodeAsync(countryCode);
				if (countryInfo == null)
				{
					return Ok(new { isValid = false, message = "Country code is valid but country information could not be retrieved" });
				}

				return Ok(new
				{
					isValid = true,
					countryCode = countryInfo.CountryCode,
					countryName = countryInfo.Name.Common,
					officialName = countryInfo.Name.Official,
					flag = countryInfo.Flag
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error validating country code {CountryCode}", countryCode);
				return StatusCode(500, new { error = "An error occurred while validating the country code" });
			}
		}
	}
}