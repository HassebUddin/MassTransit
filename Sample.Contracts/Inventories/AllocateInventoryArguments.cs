
namespace Sample.Contracts.Inventories
{
    public interface AllocateInventoryArguments
    {
        Guid OrderId { get; }
        decimal Quantity { get; }
        string ItemNumber { get; }
    }
}
