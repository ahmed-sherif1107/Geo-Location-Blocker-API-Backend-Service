using Microsoft.AspNetCore.Mvc;
using Countries.Services;
using Countries.Models;
using Countries.Dtos;
using System.Net.Http;
using System.Linq;

namespace Countries.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IPController : ControllerBase
    {
        private readonly IGeoLocationService _geoLocationService;
        private readonly IBlockedCountriesRepository _blockedCountriesRepository;
        private readonly IBlockedAttemptsRepository _blockedAttemptsRepository;
        private readonly ILogger<IPController> _logger;

        public IPController(
            IGeoLocationService geoLocationService,
            IBlockedCountriesRepository blockedCountriesRepository,
            IBlockedAttemptsRepository blockedAttemptsRepository,
            ILogger<IPController> logger)
        {
            _geoLocationService = geoLocationService;
            _blockedCountriesRepository = blockedCountriesRepository;
            _blockedAttemptsRepository = blockedAttemptsRepository;
            _logger = logger;
        }

        private async Task<string> GetPublicIpAsync()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync("https://api.ipgeolocation.io/v2/getip");
            var json = System.Text.Json.JsonDocument.Parse(response);
            return json.RootElement.GetProperty("ip").GetString()!;
        }

        private static bool IsPublicIp(string ipAddress)
        {
            if (System.Net.IPAddress.TryParse(ipAddress, out var ip))
            {
                // IPv4
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    var bytes = ip.GetAddressBytes();
                    // 10.0.0.0/8
                    if (bytes[0] == 10) return false;
                    // 172.16.0.0/12
                    if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return false;
                    // 192.168.0.0/16
                    if (bytes[0] == 192 && bytes[1] == 168) return false;
                    // 127.0.0.1
                    if (bytes[0] == 127) return false;
                }
                // IPv6
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6Multicast) return false;
                    if (ip.Equals(System.Net.IPAddress.IPv6Loopback)) return false;
                }
                return true;
            }
            return false;
        }

        private string? GetClientIp()
        {
            // Check X-Forwarded-For header first (for proxies/ngrok)
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var header = Request.Headers["X-Forwarded-For"].ToString();
                // The first IP in the list is the original client
                var ip = header.Split(',').FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(ip))
                    return ip.Trim();
            }
            // Fallback to connection remote IP
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        [HttpGet("country-lookup/{ipAddress?}")]
        public async Task<IActionResult> LookupIP([FromRoute] string? ipAddress)
        {
            try
            {
                if (string.IsNullOrEmpty(ipAddress))
                {
                    var clientIp = GetClientIp();
                    if (!string.IsNullOrEmpty(clientIp) && IsPublicIp(clientIp))
                    {
                        ipAddress = clientIp;
                    }
                    else
                    {
                        ipAddress = await GetPublicIpAsync();
                    }
                }

                var result = await _geoLocationService.GetLocationByIPAsync(ipAddress);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up IP address {IPAddress}", ipAddress);
                return StatusCode(500, new { error = "An error occurred while looking up the IP address" });
            }
        }

        [HttpGet("check-block/{ipAddress?}")]
        public async Task<IActionResult> CheckBlock([FromRoute] string? ipAddress)
        {
            try
            {
                if (string.IsNullOrEmpty(ipAddress))
                {
                    var clientIp = GetClientIp();
                    if (!string.IsNullOrEmpty(clientIp) && IsPublicIp(clientIp))
                    {
                        ipAddress = clientIp;
                    }
                    else
                    {
                        ipAddress = await GetPublicIpAsync();
                    }
                }

                var location = await _geoLocationService.GetLocationByIPAsync(ipAddress);
                var isBlocked = await _blockedCountriesRepository.IsCountryBlockedAsync(location.Location.CountryCode2);

                var attempt = new BlockedAttemptLog
                {
                    Id = Guid.NewGuid(),
                    IPAddress = location.IP,
                    Timestamp = DateTime.UtcNow,
                    CountryCode = location.Location.CountryCode2,
                    CountryName = location.Location.CountryName,
                    UserAgent = Request.Headers.UserAgent.ToString(),
                    BlockedStatus = isBlocked,
                    RequestPath = Request.Path
                };

                await _blockedAttemptsRepository.AddAttemptAsync(attempt);

                return Ok(new
                {
                    IsBlocked = isBlocked,
                    Country = new
                    {
                        Code = location.Location.CountryCode2,
                        Name = location.Location.CountryName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking block status");
                return StatusCode(500, new { error = "An error occurred while checking block status" });
            }
        }
    }
}