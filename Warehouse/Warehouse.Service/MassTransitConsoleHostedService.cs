namespace Warehouse.Service
{
    using System.Threading;
    using System.Threading.Tasks;
    using MassTransit;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class MassTransitConsoleHostedService : IHostedService
    {
        readonly IBus _bus;
        readonly ILogger<MassTransitConsoleHostedService> _logger;

        public MassTransitConsoleHostedService(IBus bus, ILogger<MassTransitConsoleHostedService> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Warehouse MassTransit bus ready at {Address}", _bus.Address);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Warehouse MassTransit bus stopping");
            return Task.CompletedTask;
        }
    }
}
