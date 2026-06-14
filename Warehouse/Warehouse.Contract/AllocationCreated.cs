

namespace Warehouse.Contract
{

    public interface AllocationCreated
    {
        Guid AllocationId { get; }
        TimeSpan HoldDuration { get; }
    }
}
