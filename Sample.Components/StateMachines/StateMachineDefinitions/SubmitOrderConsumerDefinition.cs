

using MassTransit;
using Sample.Components.Consumers;

namespace Sample.Components.StateMachines.StateMachineDefinitions
{
    public class SubmitOrderConsumerDefinition:ConsumerDefinition<SubmitOrderConsumer>
    {
        public SubmitOrderConsumerDefinition()
        {
            ConcurrentMessageLimit = 4;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<SubmitOrderConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(3,1000));
            endpointConfigurator.UseInMemoryOutbox();
        }
     
    }
}
