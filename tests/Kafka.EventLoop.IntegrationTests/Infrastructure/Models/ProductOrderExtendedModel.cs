namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Models
{
    public class ProductOrderExtendedModel : ProductOrderModel
    {
        public User? User { get; set; }
        public Product? Product { get; set; }
    }
}
