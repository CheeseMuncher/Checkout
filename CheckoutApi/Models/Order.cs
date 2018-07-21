using System.Collections.Generic;

namespace CheckoutOrderService.Models
{
    /// <summary>
    /// Representation of an order for the api client
    /// </summary>
    public class Order
    {
        public int Id { get; set; }
        public List<OrderLine> Lines { get; set; }
    }
}
