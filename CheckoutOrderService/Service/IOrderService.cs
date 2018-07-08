﻿using CheckoutOrderService.Common;
using CheckoutOrderService.Models;

namespace CheckoutOrderService
{
    /// <summary>
    /// Performs order related operations
    /// </summary>
    public interface IOrderService
    {
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
        /// Adds or updates the supplied <see cref="OrderLineModel"/> object to the order corresponding to the supplied order Id 
        /// </summary>
        /// <returns>The persistence identifier for the line</returns>
        ServiceResponse<int> UpdateOrderLine(int orderId, OrderLineModel line);
    }
}
