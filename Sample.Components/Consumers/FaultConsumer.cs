

using MassTransit;
using Sample.Contracts;

namespace Sample.Components.Consumers
{

    public class FaultConsumer :
        IConsumer<Fault<FullFillOrder>>
    {
        public Task Consume(ConsumeContext<Fault<FullFillOrder>> context)
        {
            return Task.CompletedTask;
        }
    }
}
