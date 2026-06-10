

using MassTransit;
using Microsoft.Extensions.Logging;
using Sample.Contracts;
using System.Collections.Concurrent;

namespace Sample.Components.Consumers
{
    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        private readonly ILogger<SubmitOrderConsumer> _logger;

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {

            _logger.Log(logLevel: LogLevel.Debug, "SubmitOrderConsumer:{CustomerNumber}", context.Message.CustomerNumber);
            if(context.Message.CustomerNumber.Contains("Test"))
            {
                await context.RespondAsync<OrderSubmisionRejected>(new
                {
                    InVar.Timestamp,
                    context.Message.OrderId,
                    context.Message.CustomerNumber,
                    Reason=$"test cusotmer cannot submit order {context.Message.CustomerNumber}"
                });
                return;
            }
            await context.RespondAsync<OrderSubmisionAccepted>(new
            {
                InVar.Timestamp,
               context.Message.OrderId,
               context.Message.CustomerNumber
            });

        }
    }
}
