namespace Warehouse.Components.StateMachines.StateMachineDefinitions
{
    using MassTransit;
    using Warehouse.Components.StateMachines.States;

    public class AllocateStateMachineDefinition :
        SagaDefinition<AllocationState>
    {
        public AllocateStateMachineDefinition()
        {
            ConcurrentMessageLimit = 10;
        }

        protected override void ConfigureSaga(
            IReceiveEndpointConfigurator endpointConfigurator,
            ISagaConfigurator<AllocationState> sagaConfigurator,
            IRegistrationContext context)
        {
            endpointConfigurator.UseMessageRetry(r => r.Interval(3, 1000));
            sagaConfigurator.UseInMemoryOutbox(context);
        }
    }
}
