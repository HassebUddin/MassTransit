
using MassTransit;
using System;
using Warehouse.Contract;

namespace Warehouse.Components.Consumers
{
    public class AllocateInventoryConsumer : IConsumer<AllocateInventory>
    {
        public async Task Consume(ConsumeContext<AllocateInventory> context)
        {
            await context.Publish<AllocationCreated>(new
            {
               context.Message.AllocationId,
               HoldDuration = TimeSpan.FromSeconds(15),
            });

            await context.RespondAsync<InventoryAllocate>(new
            {
                AllocationId = context.Message.AllocationId,
                Quantity=context.Message.Quantity,
               ItemNumber=context.Message.ItemNumber
            });

        }
    }
}
