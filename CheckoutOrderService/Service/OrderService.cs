using CheckoutOrderService.Common;
using CheckoutOrderService.Dependencies;
using CheckoutOrderService.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckoutOrderService
{
    /// <inheritdoc />
    public class OrderService : IOrderService
    {
        private readonly IRepository _repository;
        private readonly ILogger _logger;

        public OrderService(IRepository repository, ILogger logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <inheritdoc />
        public ServiceResponse<int> CreateNewOrder(OrderModel order = null)
        {
            try
            {
                var repoOrder = _repository.Save(order ?? new OrderModel());
                return new ServiceResponse<int>(repoOrder.Id);
            }
            catch (Exception e)
            {
                var publicErrorMessage = "Error creating new Order";
                LogError(nameof(CreateNewOrder), publicErrorMessage, $"{e.Message} StackTrace: {e.StackTrace}");
                return new ServiceResponse<int>(ServiceError.InternalServerError, publicErrorMessage);
            }
        }

        /// <inheritdoc />
        public ServiceResponse<OrderModel> GetOrder(int id)
        {
            var publicErrorMessage = $"Error fetching order with id {id}";
            try
            {
                var data = _repository.Get<OrderModel>(order => order.Id == id);
                if (data == null || !data.Any())
                {
                    return new ServiceResponse<OrderModel>(ServiceError.NotFound, "Order not found");
                }
                if (data.Count() > 1)
                {
                    LogError(nameof(GetOrder), publicErrorMessage, $"{data.Count()} duplicate matches found");
                    return new ServiceResponse<OrderModel>(ServiceError.InternalServerError, publicErrorMessage);
                }
                return new ServiceResponse<OrderModel>(data.First());
            }
            catch (Exception e)
            {
                LogError(nameof(GetOrder), publicErrorMessage, $"{e.Message} StackTrace: {e.StackTrace}");
                return new ServiceResponse<OrderModel>(ServiceError.InternalServerError, publicErrorMessage);
            }
        }

        /// <inheritdoc />
        public ServiceResponse ClearOrder(int id)
        {
            try
            {
                var orderResponse = GetOrder(id);
                if (!orderResponse.IsSuccessful)
                {
                    return new ServiceResponse(orderResponse.ServiceError, orderResponse.ErrorMessages.ToArray());
                }
                var order = orderResponse.Data;
                if (order.Lines == null || !order.Lines.Any())
                {
                    return new ServiceResponse();
                }
                order.Lines.Clear();
                _repository.Save(order);
                return new ServiceResponse();
            }
            catch (Exception e)
            {
                var publicErrorMessage = $"Error clearing order with id {id}";
                LogError(nameof(ClearOrder), publicErrorMessage, $"{e.Message} StackTrace: {e.StackTrace}");
                return new ServiceResponse(ServiceError.InternalServerError, publicErrorMessage);
            }
        }

        /// <inheritdoc />
        public ServiceResponse<int> UpdateOrderLine(int orderId, OrderLineModel line)
        {
            var publicErrorMessage = $"Error saving order with id {orderId}";
            try
            {
                var orderResponse = GetOrder(orderId);
                if (!orderResponse.IsSuccessful)
                {
                    return new ServiceResponse<int>(orderResponse.ServiceError, orderResponse.ErrorMessages.ToArray());
                }

                var order = orderResponse.Data;
                var lineValidationResponse = ValidateOrderLine(order, line, publicErrorMessage);
                if (!lineValidationResponse.IsSuccessful)
                {
                    return lineValidationResponse;
                }

                // Add a new line
                if (order.Lines.All(l => l.Id != line.Id))
                {
                    order.Lines.Add(line);
                    var repoOrder = _repository.Save(order);
                    return new ServiceResponse<int>(repoOrder.Lines.First(l => l.Sku.Id == line.Sku.Id).Id);
                }
                // update an existing line
                order.Lines.First(l => l.Id == line.Id).Quantity = line.Quantity;
                _repository.Save(orderResponse.Data);
                return new ServiceResponse<int>(line.Id);
            }
            catch (Exception e)
            {
                LogError(nameof(UpdateOrderLine), publicErrorMessage, $"{e.Message} StackTrace: {e.StackTrace}");
                return new ServiceResponse<int>(ServiceError.InternalServerError, publicErrorMessage);
            }
        }

        /// <inheritdoc />
        public ServiceResponse DeleteOrderLine(int orderId, int lineId)
        {
            try
            {
                var orderResponse = GetOrder(orderId);
                if (!orderResponse.IsSuccessful)
                {
                    return new ServiceResponse<int>(orderResponse.ServiceError, orderResponse.ErrorMessages.ToArray());
                }
                var order = orderResponse.Data;
                if (order.Lines == null || !order.Lines.Any(line => line.Id == lineId))
                {
                    return new ServiceResponse<int>(ServiceError.BadRequest, $"Error removing line from order with id {orderId}, order does not include line with id {lineId}");
                }
                order.Lines.RemoveAll(line => line.Id == lineId);
                _repository.Save(order);
                return new ServiceResponse();
            }
            catch(Exception e)
            {
                var publicErrorMessage = $"Error deleting line with {lineId} from order with id {orderId}";
                LogError(nameof(DeleteOrderLine), publicErrorMessage, $"{e.Message} StackTrace: {e.StackTrace}");
                return new ServiceResponse(ServiceError.InternalServerError, publicErrorMessage);
            }
        }
        
        /// <summary>
                 /// Validates the supplied input line against the supplied order
                 /// </summary>
        private ServiceResponse<int> ValidateOrderLine(OrderModel order, OrderLineModel line, string publicErrorMessage)
        {
            if (order.Lines == null)
            {
                order.Lines = new List<OrderLineModel>();
            }
            if (order.Lines.Where(l => l.Id == line.Id).Count() > 1)
            {
                LogError(nameof(UpdateOrderLine), publicErrorMessage, $"{order.Lines.Where(l => l.Id == line.Id).Count()} duplicate line matches found for order line id {line.Id}");
                return new ServiceResponse<int>(ServiceError.InternalServerError, publicErrorMessage);
            }
            if (order.Lines.All(l => l.Id != line.Id))
            {
                if (order.Lines.Any(l => l.Sku.Id == line.Sku.Id))
                {
                    return new ServiceResponse<int>(ServiceError.BadRequest, $"Order already contains a line with Sku Code {line.Sku.Id}");
                }
                var skuValidationResponse = ValidateNewLineSku(order.Id, line.Sku.Id, publicErrorMessage);
                if (!skuValidationResponse.IsSuccessful)
                {
                    return skuValidationResponse;
                }
            }
            return new ServiceResponse<int>();
        }

        /// <summary>
        /// Validates the supplied sku id
        /// </summary>
        private ServiceResponse<int> ValidateNewLineSku(int orderId, string skuId, string publicErrorMessage)
        {
            var repoSkus = _repository.Get<SkuModel>(sku => sku.Id == skuId);
            if (repoSkus == null || !repoSkus.Any())
            {
                return new ServiceResponse<int>(ServiceError.BadRequest, $"Error saving order with id {orderId}, sku with code {skuId} not found");
            }
            if (repoSkus.Count() > 1)
            {
                LogError(nameof(UpdateOrderLine), publicErrorMessage, $"{repoSkus.Count()} duplicate matches found for sku id {skuId}");
                return new ServiceResponse<int>(ServiceError.InternalServerError, publicErrorMessage);
            }
            return new ServiceResponse<int>();
        }

        /// <summary>
        /// To ensure consistent logging
        /// </summary>
        private void LogError(string method, string publicMessage, string privateMessage)
        {
            _logger.LogError($"{nameof(OrderService)}.{method}: {publicMessage}. Reason: {privateMessage}");
        }
    }
}
