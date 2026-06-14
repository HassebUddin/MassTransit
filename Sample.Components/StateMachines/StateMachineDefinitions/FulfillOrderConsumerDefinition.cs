using MassTransit;
using Sample.Components.Consumers;

namespace Sample.Components.StateMachines.StateMachineDefinitions
{

    public class FulfillOrderConsumerDefinition :
        ConsumerDefinition<FullFillOrderConsumer>
    {
        public FulfillOrderConsumerDefinition()
        {
            ConcurrentMessageLimit = 20;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<FullFillOrderConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r =>
            {
                r.Ignore<InvalidOperationException>();

                r.Interval(3, 1000);
            });

            endpointConfigurator.DiscardFaultedMessages();
        }
    }
}
