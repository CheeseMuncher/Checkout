using System.Collections.Generic;

namespace CheckoutOrderService.Models
{
    /// <summary>
    /// Representation of an order for the api client
    /// </summary>
    public class Order
    {
        /// <summary>
        /// The identifer of the order
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The collection of order lines
        /// </summary>
        public List<OrderLine> Lines { get; set; }
    }
}
