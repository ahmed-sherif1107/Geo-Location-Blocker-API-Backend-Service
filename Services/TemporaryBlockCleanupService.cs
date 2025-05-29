using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Countries.Services
{
    public class TemporaryBlockCleanupService : BackgroundService
    {
        private readonly IBlockedCountriesRepository _blockedCountriesRepository;
        private readonly ILogger<TemporaryBlockCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

        public TemporaryBlockCleanupService(
            IBlockedCountriesRepository blockedCountriesRepository,
            ILogger<TemporaryBlockCleanupService> logger)
        {
            _blockedCountriesRepository = blockedCountriesRepository;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting temporary block cleanup");
                    await _blockedCountriesRepository.RemoveExpiredTemporaryBlocksAsync();
                    _logger.LogInformation("Completed temporary block cleanup");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up temporary blocks");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }
}