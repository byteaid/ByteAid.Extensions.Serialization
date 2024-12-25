namespace ByteAid.Extensions.Serialization.Tests.Models
{
    public class Order
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
