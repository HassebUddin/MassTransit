namespace Warehouse.Components.StateMachines
{
    using MassTransit;
    using Microsoft.Extensions.Logging;
    using System;
    using Warehouse.Components.StateMachines.States;
    using Warehouse.Contract;

    public class AllocationStateMachine :
        MassTransitStateMachine<AllocationState>
    {
        public AllocationStateMachine(ILogger<AllocationStateMachine> logger)
        {
            Event(() => AllocationCreated, x => x.CorrelateById(m => m.Message.AllocationId));
            Event(() => ReleaseRequested, x => x.CorrelateById(m => m.Message.AllocationId));

            Schedule(() => HoldExpiration, x => x.HoldDurationToken, s =>
            {
                s.Delay = TimeSpan.FromHours(1);
                s.Received = x => x.CorrelateById(m => m.Message.AllocationId);
            });

            InstanceState(x => x.CurrentState);

            Initially(
                When(AllocationCreated)
                    .Schedule(HoldExpiration, context => context.Init<AllocationHoldDurationExpired>(new { context.Message.AllocationId }),
                        context => context.Message.HoldDuration)
                    .TransitionTo(Allocated),
                When(ReleaseRequested)
                    .TransitionTo(Released)
            );

            During(Allocated,
                When(AllocationCreated)
                    .Then(context => logger.LogInformation("Allocation already allocated: {AllocationId}", context.Saga.CorrelationId)),
                When(HoldExpiration.Received!)
                    .Then(context => logger.LogInformation("Allocation expired {AllocationId}", context.Saga.CorrelationId))
                    .Finalize(),
                When(ReleaseRequested)
                    .Unschedule(HoldExpiration)
                    .Then(context => logger.LogInformation("Allocation Release Granted: {AllocationId}", context.Saga.CorrelationId))
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }

        public Schedule<AllocationState, AllocationHoldDurationExpired> HoldExpiration { get; private set; } = null!;

        public State Allocated { get; private set; } = null!;
        public State Released { get; private set; } = null!;

        public Event<AllocationCreated> AllocationCreated { get; private set; } = null!;
        public Event<AllocationReleaseRequested> ReleaseRequested { get; private set; } = null!;
    }
}
