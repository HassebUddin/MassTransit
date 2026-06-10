namespace Sample.Service
{
    using System.Threading;
    using System.Threading.Tasks;
    using MassTransit;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Tutorial-style hosted service. In MassTransit 8 the bus auto-starts with the host,
    /// so this logs readiness instead of calling IBusControl.StartAsync (MT7 API).
    /// </summary>
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
            _logger.LogInformation("MassTransit bus ready at {Address}", _bus.Address);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MassTransit bus stopping");
            return Task.CompletedTask;
        }
    }
}
