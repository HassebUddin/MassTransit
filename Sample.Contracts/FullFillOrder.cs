

namespace Sample.Contracts
{
    public interface FullFillOrder
    {
        Guid OrderId { get; }
        string CustomerNumber { get; }
        string PaymentCardNumber { get; }
    }
}
