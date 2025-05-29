using Microsoft.AspNetCore.Mvc;
using Countries.Services;
using Countries.Dtos;

namespace Countries.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly IBlockedAttemptsRepository _blockedAttemptsRepository;
        private readonly ILogger<LogsController> _logger;

        public LogsController(
            IBlockedAttemptsRepository blockedAttemptsRepository,
            ILogger<LogsController> logger)
        {
            _blockedAttemptsRepository = blockedAttemptsRepository;
            _logger = logger;
        }

        [HttpGet("blocked-attempts")]
        public async Task<IActionResult> GetBlockedAttempts([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _blockedAttemptsRepository.GetBlockedAttemptsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving blocked attempts");
                return StatusCode(500, new { error = "An error occurred while retrieving blocked attempts" });
            }
        }
    }
}