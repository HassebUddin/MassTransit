

namespace Warehouse.Contract
{
    public interface AllocateInventory
    {
        Guid AllocationId { get; }
        decimal Quantity { get; }
        string ItemNumber { get; }
    }
}
