
using MassTransit;
using Warehouse.Contract;

namespace Warehouse.Components.Consumers
{
    public class AllocateInventoryConsumer : IConsumer<AllocateInventory>
    {
        public async Task Consume(ConsumeContext<AllocateInventory> context)
        {
            await Task.Delay(500);
            await context.RespondAsync<InventoryAllocate>(new
            {
                AllocationId = context.Message.AllocationId,
                Quantity=context.Message.Quantity,
               ItemNumber=context.Message.ItemNumber
            });

        }
    }
}
