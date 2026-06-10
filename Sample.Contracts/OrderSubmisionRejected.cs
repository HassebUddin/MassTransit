namespace Sample.Components.Consumers
{
    public interface OrderSubmisionRejected
    {
        public Guid OrderId { get; }
        public DateTime TimeStamp { get; }
        public string CustomerNumber { get; }
        public string Reason { get; }
    }
}