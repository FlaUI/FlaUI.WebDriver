using Microsoft.Extensions.Options;

namespace FlaUI.WebDriver
{
    public class SessionCleanupService : IHostedService, IDisposable
    {
        private readonly ILogger<SessionCleanupService> _logger;
        private Timer? _timer = null;

        public IServiceProvider Services { get; }
        public IOptions<SessionCleanupOptions> Options { get; }

        public SessionCleanupService(IServiceProvider services, IOptions<SessionCleanupOptions> options, ILogger<SessionCleanupService> logger)
        {
            Services = services;
            Options = options;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Session cleanup service running every {SchedulingIntervalSeconds} seconds", Options.Value.SchedulingIntervalSeconds);

            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(Options.Value.SchedulingIntervalSeconds), TimeSpan.FromSeconds(Options.Value.SchedulingIntervalSeconds));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            using (var scope = Services.CreateScope())
            {
                var sessionRepository =
                scope.ServiceProvider
                    .GetRequiredService<ISessionRepository>();

                var timedOutSessions = sessionRepository.FindTimedOut();
                if(timedOutSessions.Count > 0)
                {
                    _logger.LogInformation("Session cleanup service cleaning up {Count} sessions that did not receive commands in their specified new command timeout interval", timedOutSessions.Count);

                    foreach (Session session in timedOutSessions)
                    {
                        sessionRepository.Delete(session);
                        session.Dispose();
                    }
                }
                else
                {
                    _logger.LogInformation("Session cleanup service did not find sessions to cleanup");
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Session cleanup service is stopping");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
