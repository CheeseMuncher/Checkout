namespace CheckoutOrderService.Models
{
    /// <summary>
    /// Representation of an order line in the persistence layer
    /// </summary>
    public class OrderLineModel
    {
        public OrderLineModel(int id, SkuModel sku)
        {
            Id = id;
            Sku = sku;
        }

        public int Id { get; }
        public int SortOrder { get; set; }
        public SkuModel Sku { get; }
        public int Quantity { get; set; }
    }
}
