using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BarqTMS.API.Services
{
    public class OverdueTaskNotificationService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
