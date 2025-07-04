using StreamitMVC.Services.Interfaces;

namespace StreamitMVC.Services
{

    public class SessionCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public SessionCleanupBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var cleaner = scope.ServiceProvider.GetRequiredService<ISessionCleanerService>();
                    await cleaner.CleanOldSessionsAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
