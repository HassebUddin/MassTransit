

using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.Extensions.Logging;

namespace Sample.Components.BatchConsumers
{

    public class RoutingSlipBatchEventConsumer :
        IConsumer<Batch<RoutingSlipCompleted>>
    {
        readonly ILogger<RoutingSlipBatchEventConsumer> _logger;

        public RoutingSlipBatchEventConsumer(ILogger<RoutingSlipBatchEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Batch<RoutingSlipCompleted>> context)
        {
            
                _logger.Log(LogLevel.Information, "Routing Slips Completed: {TrackingNumbers}",
                    string.Join(", ", context.Message.Select(x => x.Message.TrackingNumber)));

            return Task.CompletedTask;
        }
    }
}
