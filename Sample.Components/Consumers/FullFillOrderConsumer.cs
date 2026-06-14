
using MassTransit;
using MassTransit.Courier.Contracts;
using Sample.Contracts;

namespace Sample.Components.Consumers
{
    public class FullFillOrderConsumer : IConsumer<FullFillOrder>
    {
        public async Task Consume(ConsumeContext<FullFillOrder> context)
        {
            if (context.Message.CustomerNumber?.StartsWith("INVALID") == true)
            {
                throw new InvalidOperationException("We tried, but the customer is invalid");
            }

            var builder = new RoutingSlipBuilder(NewId.NextGuid());

            builder.AddActivity("AllocateInventory", new Uri("queue:allocate-inventory_execute"), new
            {
                OrderId = context.Message.OrderId,
                ItemNumber = "ITEM123",
                Quantity = 10.0m
            });

            var testnumber = context.Message.PaymentCardNumber;
            builder.AddActivity("PaymentActivity", new Uri("queue:payment_execute"),
             new
             {
                 CardNumber = context.Message.PaymentCardNumber ?? "5999-1234-5678-9012",
                 Amount = 99.95m
             });


            builder.AddVariable("OrderId", context.Message.OrderId);

            builder.AddSubscription(new Uri("queue:routing-slip-event"),
                RoutingSlipEvents.ActivityCompleted | RoutingSlipEvents.Faulted | RoutingSlipEvents.Supplemental,
                RoutingSlipEventContents.All);

            builder.AddSubscription(context.SourceAddress,
                RoutingSlipEvents.Faulted | RoutingSlipEvents.Supplemental,
                RoutingSlipEventContents.None, x => x.Send<OrderFulfillmentFaulted>(new { context.Message.OrderId }));

            builder.AddSubscription(context.SourceAddress,
                RoutingSlipEvents.Completed | RoutingSlipEvents.Supplemental,
                RoutingSlipEventContents.None, x => x.Send<OrderFulfillmentCompleted>(new { context.Message.OrderId }));

            var routingSlip = builder.Build();

            await context.Execute(routingSlip);
        }
    }
}
