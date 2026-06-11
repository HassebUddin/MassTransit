

namespace Warehouse.Contract
{
    public interface InventoryAllocate
    {
        Guid AllocationId { get; }
        decimal Quantity { get; }
        string ItemNumber { get; }
    }
}
