

namespace Sample.Contracts
{
    public interface SubmitOrder
    {
        public Guid OrderId { get; }
        public DateTime TimeStamp { get;  }
        public string CustomerNumber { get;  }
    }
}
