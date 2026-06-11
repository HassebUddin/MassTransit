

namespace Warehouse.Contract
{

    public interface AllocationReleaseRequested
    {
        Guid AllocationId { get; }

        string Reason { get; }
    }
}
