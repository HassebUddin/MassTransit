

using MassTransit;
using MassTransit.Transports;
using Microsoft.Extensions.Logging;
using Sample.Contracts;
using System.Collections.Concurrent;

namespace Sample.Components.Consumers
{
    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        private readonly ILogger<SubmitOrderConsumer> _logger;
        readonly IPublishEndpoint _publishEndpoint;

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
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


            MessageData<string> notes = context.Message.Notes;
            if (notes?.HasValue ?? false)
            {
                string notesValue = await notes.Value;

                Console.WriteLine("NOTES: {0}", notesValue);
            }


            await context.Publish<OrderSubmitted>(new
            {
                context.Message.OrderId,
                Timestamp = context.Message.TimeStamp,
                context.Message.CustomerNumber,
                context.Message.PaymentCardNumber,
                context.Message.Notes

            });
            if (context.RequestId != null)
            {
                await context.RespondAsync<OrderSubmisionAccepted>(new
                {
                    InVar.Timestamp,
                    context.Message.OrderId,
                    context.Message.CustomerNumber
                });

            }
          

        }
    }
}
