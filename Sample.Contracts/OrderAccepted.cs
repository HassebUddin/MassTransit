
namespace Sample.Contracts
{
    public interface OrderAccepted
    {
        public Guid OrderId { get; }
        public DateTime TimeStamp { get; }
    }
}
