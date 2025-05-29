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
                    _logger.LogInformation("{Time}: Starting temporary block cleanup", DateTimeOffset.Now);
                    await _blockedCountriesRepository.RemoveExpiredTemporaryBlocksAsync();
                    _logger.LogInformation("{Time}: Completed temporary block cleanup", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Time}: Error occurred while cleaning up temporary blocks", DateTimeOffset.Now);
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }
}