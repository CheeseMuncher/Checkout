namespace CheckoutOrderService.Models
{
    /// <summary>
    /// Representation of an order line for the api client
    /// </summary>
    public class OrderLine
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public int Quantity { get; set; }
        public string SkuCode { get; set; }
        public string SkuDisplayName { get; set; }
    }
}
