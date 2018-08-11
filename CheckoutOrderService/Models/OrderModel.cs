using System.Collections.Generic;

namespace CheckoutOrderService.Models
{
    /// <summary>
    /// Representation of an order in the persistence layer
    /// </summary>
    public class OrderModel
    {
        public OrderModel(int id)
        {
            Id = id;
        }

        public int Id { get; }
        public List<OrderLineModel> Lines { get; set; }
    }
}
