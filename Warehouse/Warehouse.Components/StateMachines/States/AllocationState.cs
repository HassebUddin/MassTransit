
namespace Warehouse.Components.StateMachines.States
{
    using Automatonymous;
    using MassTransit;
    using MassTransit.Saga;
    using System;
    using MongoDB.Bson.Serialization.Attributes;


    public class AllocationState :
        SagaStateMachineInstance,
        ISagaVersion
    {
        public string? CurrentState { get; set; }

        public Guid? HoldDurationToken { get; set; }

        public int Version { get; set; }

        [BsonId]
        public Guid CorrelationId { get; set; }
    }
}
