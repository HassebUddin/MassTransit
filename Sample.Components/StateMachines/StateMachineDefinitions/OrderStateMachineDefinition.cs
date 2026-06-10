

using MassTransit;
using Sample.Components.StateMachines.States;

namespace Sample.Components.StateMachines.StateMachineDefinitions
{
    public class OrderStateMachineDefinition:SagaDefinition<OrderState>
    {
        public OrderStateMachineDefinition()
        {
            ConcurrentMessageLimit = 4;
        }
        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<OrderState> sagaConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 5000, 1000));
           endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
