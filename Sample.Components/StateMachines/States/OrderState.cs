using MassTransit;
using MongoDB.Bson.Serialization.Attributes;

namespace Sample.Components.StateMachines.States
{
    public class OrderState :
        SagaStateMachineInstance,
        ISagaVersion
    {
        public string? CurrentState { get; set; }

        public string? CustomerNumber { get; set; }
        public string? PaymentCardNumber { get; set; }

        public string? FaultReason { get; set; }

        public DateTime? SubmitDate { get; set; }
        public DateTime? Updated { get; set; }

        public int Version { get; set; }

        [BsonId]
        public Guid CorrelationId { get; set; }
    }
}
