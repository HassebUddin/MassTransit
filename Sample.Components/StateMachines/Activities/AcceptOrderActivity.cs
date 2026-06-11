

using Automatonymous;
using MassTransit;
using Sample.Components.StateMachines.States;
using Sample.Contracts;

namespace Sample.Components.StateMachines.Activities
{
    public class AcceptOrderActivity : IStateMachineActivity<OrderState, OrderAccepted>
    {
        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderState, OrderAccepted> context, IBehavior<OrderState, OrderAccepted> next)
        {
            Console.WriteLine($"Hello,World, order is {context.Message.OrderId}");
            await next.Execute(context).ConfigureAwait(false);
        }

        public async Task Faulted<TException>(BehaviorExceptionContext<OrderState, OrderAccepted, TException> context, IBehavior<OrderState, OrderAccepted> next) where TException : Exception
        {
            await next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("accept-order");
        }
    }
}
