

using Automatonymous;
using MassTransit;
using MassTransit.Saga;
using MongoDB.Bson.Serialization.Attributes;
using Sample.Components.StateMachines.Activities;
using Sample.Components.StateMachines.States;
using Sample.Contracts;
using System;


namespace Sample.Components.StateMachines
{


    public class OrderStateMachine :
        MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderAccepted, x => x.CorrelateById(m => m.Message.OrderId));
            //Event(() => FulfillmentCompleted, x => x.CorrelateById(m => m.Message.OrderId));
            //Event(() => FulfillmentFaulted, x => x.CorrelateById(m => m.Message.OrderId));
            //Event(() => FulfillOrderFaulted, x => x.CorrelateById(m => m.Message.Message.OrderId));
            Event(() => OrderStatusRequested, x =>
            {
                x.CorrelateById(m => m.Message.OrderId);
                x.OnMissingInstance(m => m.ExecuteAsync(async context =>
                {
                    if (context.RequestId.HasValue)
                    {
                        await context.RespondAsync<OrderNotFound>(new { context.Message.OrderId });
                    }
                }));
            });
            Event(() => AccountClosed, x =>
                x.CorrelateBy(s => s.CustomerNumber, m => m.Message.CustomerNumber));

            InstanceState(x => x.CurrentState);
            Initially(
                When(OrderSubmitted)
                    .Then(context =>
                    {
                        context.Saga.SubmitDate = context.Message.TimeStamp;
                        context.Saga.CustomerNumber = context.Message.CustomerNumber;
                        context.Saga.Updated = DateTime.UtcNow;
                    })
                    .TransitionTo(Submitted));

            During(Submitted,
                Ignore(OrderSubmitted),
                When(AccountClosed)
                    .TransitionTo(Canceled),
                When(OrderAccepted)
                    .Activity(x => x.OfType<AcceptOrderActivity>())
                    .TransitionTo(Accepted)
                    );

            //During(Accepted,
            //    When(FulfillOrderFaulted)
            //        .Then(context => Console.WriteLine("Fulfill Order Faulted: {0}", context.Data.Exceptions.FirstOrDefault()?.Message))
            //        .TransitionTo(Faulted),
            //    When(FulfillmentFaulted)
            //        .TransitionTo(Faulted),
            //    When(FulfillmentCompleted)
            //        .TransitionTo(Completed));


            DuringAny(
                When(OrderStatusRequested)
                   .RespondAsync(x => x.Init<OrderStatus>(new
                   {
                       OrderId = x.Saga.CorrelationId,
                       State = x.Saga.CurrentState
                   })));
        }

        public State Submitted { get; private set; }
        public State Accepted { get; private set; }
        public State Canceled { get; private set; }
        //public State Faulted { get; private set; }
        //public State Completed { get; private set; }

        public Event<SubmitOrder> OrderSubmitted { get; private set; }
        public Event<OrderAccepted> OrderAccepted { get; private set; }
        //public Event<OrderFulfillmentCompleted> FulfillmentCompleted { get; private set; }
        //public Event<OrderFulfillmentFaulted> FulfillmentFaulted { get; private set; }
        public Event<CheckOrder> OrderStatusRequested { get; private set; }
        public Event<CustomerAccountClosed> AccountClosed { get; private set; }
        //public Event<Fault<FulfillOrder>> FulfillOrderFaulted { get; private set; }
    }
}
