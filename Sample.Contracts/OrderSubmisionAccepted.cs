

namespace Sample.Contracts
{
    public interface OrderSubmisionAccepted
    {
        public Guid OrderId { get; }
        public DateTime TimeStamp { get; }
        public string CustomerNumber { get; }

    }
}
