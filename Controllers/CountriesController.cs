using Microsoft.AspNetCore.Mvc;
using Countries.Services;
using Countries.Models;
using Countries.Dtos;
using Countries.Enums;

namespace Countries.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly IBlockedCountriesRepository _blockedCountriesRepository;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(
            IBlockedCountriesRepository blockedCountriesRepository,
            ILogger<CountriesController> logger)
        {
            _blockedCountriesRepository = blockedCountriesRepository;
            _logger = logger;
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

                var country = new BlockedCountry
                {
                    CountryCode = request.CountryCode.ToUpper(),
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

                var result = await _blockedCountriesRepository.RemoveBlockedCountryAsync(countryCode.ToUpper());
                if (!result)
                {
                    return NotFound(new { error = "Country is not blocked" });
                }

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

                var country = new BlockedCountry
                {
                    CountryCode = request.CountryCode.ToUpper(),
                    BlockedAt = DateTime.UtcNow,
                    IsTemporary = true,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(request.DurationMinutes.Value)
                };

                var result = await _blockedCountriesRepository.AddBlockedCountryAsync(country);
                if (!result)
                {
                    return Conflict(new { error = "Country is already blocked" });
                }

                return Ok(country);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error temporarily blocking country {CountryCode}", request.CountryCode);
                return StatusCode(500, new { error = "An error occurred while temporarily blocking the country" });
            }
        }
    }
}