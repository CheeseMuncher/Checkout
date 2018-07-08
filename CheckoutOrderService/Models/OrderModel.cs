using System.Collections.Generic;

namespace CheckoutOrderService.Models
{
    /// <summary>
    /// Representation of an order in the persistence layer
    /// </summary>
    public class OrderModel
    {
        public int Id { get; set; }
        public List<OrderLineModel> Lines { get; set; }
    }
}
