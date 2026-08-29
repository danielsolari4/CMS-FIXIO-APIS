using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ray.Managers;
using Ray.Utils.Logging;

namespace Ray.BackendApi.Services
{
    public class CacheInvalidationQueueHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CacheInvalidationQueueHostedService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var manager = scope.ServiceProvider.GetRequiredService<ICacheInvalidationManager>();

                    await manager.RecoverQueue();
                    await manager.ProcessQueue();
                }
                catch (Exception ex)
                {
                    CMSLogger.Error($"[cache-invalidation] hosted service failed: {ex}");
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }
}
