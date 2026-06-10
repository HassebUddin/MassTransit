
namespace Sample.Contracts
{
    public interface OrderStatus
    {
        public Guid OrderId { get; }
        public string State { get;  }
    }
}
