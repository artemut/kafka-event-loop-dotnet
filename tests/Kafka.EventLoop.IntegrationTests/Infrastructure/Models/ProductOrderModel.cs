namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Models
{
    public class ProductOrderModel
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public DateTimeOffset OrderedAt { get; set; }
        public double Price { get; set; }
        public Currency Currency { get; set; }
        public bool HasDelivery { get; set; }
        public DeliveryDetails? DeliveryDetails { get; set; }
        public string? Comments { get; set; }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
