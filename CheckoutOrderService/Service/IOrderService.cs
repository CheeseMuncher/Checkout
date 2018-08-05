using CheckoutOrderService.Common;
using CheckoutOrderService.Models;
using System.Collections.Generic;

namespace CheckoutOrderService
{
    /// <summary>
    /// Performs order related operations
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Returns a collection of <see cref="SkuModel"/> objects
        /// </summary>
        ServiceResponse<IEnumerable<SkuModel>> GetSkus();

        /// <summary>
        /// Returns a collection of <see cref="OrderModel"/> objects
        /// </summary>
        ServiceResponse<IEnumerable<OrderModel>> GetOrders();

        /// <summary>
        /// Creates a persistent <see cref="OrderModel"/> object to track changes after an order is created
        /// </summary>
        /// <param name="order">The new order to be created, an empty order otherwise</param>
        /// <returns>The persistence identifier</returns>
        ServiceResponse<int> CreateNewOrder(OrderModel order = null);

        /// <summary>
        /// Fetches the order corresponding to the supplied id value
        /// </summary>
        ServiceResponse<OrderModel> GetOrder(int id);

        /// <summary>
        /// Removes all order lines for the order corresponding to the supplied id value
        /// </summary>
        ServiceResponse ClearOrder(int id);

        /// <summary>
        /// Adds or updates the supplied <see cref="OrderLineModel"/> object to the order corresponding to the supplied order Id 
        /// </summary>
        /// <returns>The persistence identifier for the line</returns>
        ServiceResponse<int> UpdateOrderLine(int orderId, OrderLineModel line);

        /// <summary>
        /// Removes the corresponding line from the corresponding order for the supplied identifiers
        /// </summary>
        ServiceResponse DeleteOrderLine(int orderId, int lineId);
    }
}
