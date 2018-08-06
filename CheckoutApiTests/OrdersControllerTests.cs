using AutoMapper;
using CheckoutApi.Controllers;
using CheckoutApi.Models;
using CheckoutOrderService;
using CheckoutOrderService.Common;
using CheckoutOrderService.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace CheckoutApiTests
{
    [TestClass]
    public class OrdersControllerTests
    {
        private OrdersController _target;

        private Mock<IOrderService> _mockOrderService;
        private Mock<IMapper> _mockOrderMapper;
        private Mock<IValidator<Order>> _mockOrderValidator;
        private Mock<IValidator<OrderLine>> _mockOrderLineValidator;

        private const int orderId = 11;
        private const int lineId = 13;

        private string Message(int? i = null) => $"Error Message{(i == null ? string.Empty : $" {i}")}";

        [TestInitialize]
        public void TestInit()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockOrderMapper = new Mock<IMapper>();
            _mockOrderValidator = new Mock<IValidator<Order>>();
            _mockOrderLineValidator = new Mock<IValidator<OrderLine>>();

            _target = new OrdersController(_mockOrderService.Object, _mockOrderMapper.Object, _mockOrderValidator.Object, _mockOrderLineValidator.Object);
        }

        #region GetSkus Tests

        [TestMethod]
        public void GetSkus_InvokesServiceGet()
        {
            // Arrange
            _mockOrderService.Setup(service => service.GetSkus()).Returns(new ServiceResponse<IEnumerable<SkuModel>>());

            // Act
            _target.GetSkus();

            // Assert
            _mockOrderService.Verify(service => service.GetSkus(), Times.Once);
        }

        [TestMethod]
        public void GetSkus_ReturnsInternalServerErrorWithoutErrorMessages_IfServiceResultIsInternalServerError()
        {
            // Arrange
            _mockOrderService
                .Setup(service => service.GetSkus())
                .Returns(new ServiceResponse<IEnumerable<SkuModel>>(ServiceError.InternalServerError, Message()));

            // Act
            var result = _target.GetSkus().Result as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(result.Value?.ToString()));
        }

        [TestMethod]
        public void GetSkus_InvokesMapperWithServiceResult()
        {
            // Arrange
            var sku = new SkuModel("Test1", "Test2");
            _mockOrderService
                .Setup(service => service.GetSkus())
                .Returns(new ServiceResponse<IEnumerable<SkuModel>>(new[] { sku }));

            IEnumerable<SkuModel> mapperInput = null;
            _mockOrderMapper
                .Setup(mapper => mapper.Map<IEnumerable<SkuModel>, IEnumerable<Sku>>(It.IsAny<IEnumerable<SkuModel>>()))
                .Callback((IEnumerable<SkuModel> sm) => mapperInput = sm);

            // Act
            _target.GetSkus();

            // Assert
            Assert.IsNotNull(mapperInput);
            var result = mapperInput.ToArray();
            Assert.AreEqual(1, mapperInput.Count());
            Assert.AreEqual(sku.Id, mapperInput.First().Id);
            Assert.AreEqual(sku.DisplayName, mapperInput.First().DisplayName);
        }

        [TestMethod]
        public void GetSkus_ReturnsOkWithData_IfServiceResponseSuccessful()
        {
            // Arrange
            _mockOrderService.Setup(service => service.GetSkus()).Returns(new ServiceResponse<IEnumerable<SkuModel>>());

            var sku1 = new Sku { Code = "Test1" };
            var sku2 = new Sku { Code = "Test2" };
            var mapperResult = new List<Sku> { sku1, sku2 };
            _mockOrderMapper
                .Setup(mapper => mapper.Map<IEnumerable<SkuModel>, IEnumerable<Sku>>(It.IsAny<IEnumerable<SkuModel>>()))
                .Returns(mapperResult);            

            // Act
            var result = _target.GetSkus().Result as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
            var skus = result.Value as IEnumerable<Sku>;
            Assert.IsNotNull(skus);
            Assert.AreEqual(2, skus.Count());
            Assert.IsTrue(skus.Any(order => order.Code == sku1.Code));
            Assert.IsTrue(skus.Any(order => order.Code == sku2.Code));
        }

        #endregion GetSkus Tests

        #region GetOrders Tests

        [TestMethod]
        public void GetOrders_InvokesServiceGet()
        {
            // Arrange
            _mockOrderService.Setup(service => service.GetOrders()).Returns(new ServiceResponse<IEnumerable<OrderModel>>());

            // Act
            _target.Get();

            // Assert
            _mockOrderService.Verify(service => service.GetOrders(), Times.Once);
        }

        [TestMethod]
        public void GetOrders_ReturnsInternalServerErrorWithoutErrorMessages_IfServiceResultIsInternalServerError()
        {
            // Arrange
            _mockOrderService
                .Setup(service => service.GetOrders())
                .Returns(new ServiceResponse<IEnumerable<OrderModel>>(ServiceError.InternalServerError, Message()));

            // Act
            var result = _target.Get().Result as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(result.Value?.ToString()));
        }

        [TestMethod]
        public void GetOrders_InvokesMapperWithServiceResult()
        {
            // Arrange
            var order = new OrderModel { Id = orderId };
            _mockOrderService
                .Setup(service => service.GetOrders())
                .Returns(new ServiceResponse<IEnumerable<OrderModel>>(new[] { order }));

            IEnumerable<OrderModel> mapperInput = null;
            _mockOrderMapper
                .Setup(mapper => mapper.Map<IEnumerable<OrderModel>, IEnumerable<Order>>(It.IsAny<IEnumerable<OrderModel>>()))
                .Callback((IEnumerable<OrderModel> om) => mapperInput = om);

            // Act
            _target.Get();

            // Assert
            Assert.IsNotNull(mapperInput);
            var result = mapperInput.ToArray();
            Assert.AreEqual(1, mapperInput.Count());
            Assert.AreEqual(orderId, mapperInput.First().Id);
        }

        [TestMethod]
        public void GetOrders_ReturnsOkWithData_IfServiceResponseSuccessful()
        {
            // Arrange
            _mockOrderService.Setup(service => service.GetOrders()).Returns(new ServiceResponse<IEnumerable<OrderModel>>());

            var mapperResult = new List<Order> { new Order { Id = orderId }, new Order { Id = orderId + 1 } };
            _mockOrderMapper
                .Setup(mapper => mapper.Map<IEnumerable<OrderModel>, IEnumerable<Order>>(It.IsAny<IEnumerable<OrderModel>>()))
                .Returns(mapperResult);

            // Act
            var result = _target.Get().Result as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
            var orders = result.Value as IEnumerable<Order>;
            Assert.IsNotNull(orders);
            Assert.AreEqual(2, orders.Count());
            Assert.IsTrue(orders.Any(order => order.Id == orderId));
            Assert.IsTrue(orders.Any(order => order.Id == orderId + 1));
        }

        #endregion GetOrders Tests

        #region GetOrder Tests

        [TestMethod]
        public void GetOrder_InvokesServiceGetWithCorrectArguments()
        {
            // Arrange
            int id = 0;
            _mockOrderService
                .Setup(service => service.GetOrder(It.IsAny<int>()))
                .Callback((int i) => id = i)
                .Returns(new ServiceResponse<OrderModel>());

            // Act
            _target.Get(orderId);

            // Assert
            Assert.AreEqual(orderId, id);
        }

        [TestMethod]
        public void GetOrder_ReturnsNotFoundWithErrorMessages_IfServiceResultIsNotFound()
        {
            // Arrange
            _mockOrderService
                .Setup(service => service.GetOrder(It.IsAny<int>()))
                .Returns(new ServiceResponse<OrderModel>(ServiceError.NotFound, Message(1), Message(2)));

            // Act
            var result = _target.Get(orderId).Result as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.NotFound, result.StatusCode);
            Assert.IsTrue(result.Value.ToString().Contains(Message(1)));
            Assert.IsTrue(result.Value.ToString().Contains(Message(2)));
        }

        [TestMethod]
        public void GetOrder_ReturnsInternalServerErrorWithoutErrorMessages_IfServiceResultIsInternalServerError()
        {
            // Arrange
            _mockOrderService
                .Setup(service => service.GetOrder(It.IsAny<int>()))
                .Returns(new ServiceResponse<OrderModel>(ServiceError.InternalServerError, Message()));

            // Act
            var result = _target.Get(orderId).Result as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(result.Value?.ToString()));
        }

        [TestMethod]
        public void GetOrder_InvokesMapperWithServiceResult()
        {
            // Arrange
            var order = new OrderModel { Id = orderId };
            _mockOrderService
                .Setup(service => service.GetOrder(It.IsAny<int>()))
                .Returns(new ServiceResponse<OrderModel>(order));

            OrderModel mapperInput = null;
            _mockOrderMapper
                .Setup(mapper => mapper.Map<OrderModel, Order>(It.IsAny<OrderModel>()))
                .Callback((OrderModel om) => mapperInput = om);

            // Act
            _target.Get(orderId);

            // Assert
            Assert.IsNotNull(mapperInput);
            Assert.AreEqual(orderId, mapperInput.Id);
        }

        [TestMethod]
        public void GetOrder_ReturnsOkWithData_IfServiceResponseSuccessful()
        {
            // Arrange
            _mockOrderService.Setup(service => service.GetOrder(It.IsAny<int>())).Returns(new ServiceResponse<OrderModel>());

            var mapperResult = new Order { Id = orderId };
            _mockOrderMapper
                .Setup(mapper => mapper.Map<OrderModel, Order>(It.IsAny<OrderModel>()))
                .Returns(mapperResult);

            // Act
            var result = _target.Get(orderId).Result as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
            var order = result.Value as Order;
            Assert.IsNotNull(order);
            Assert.AreEqual(orderId, order.Id);
        }

        #endregion GetOrder Tests

        #region CreateOrder Tests

        [TestMethod]
        public void CreateOrder_ValidatesInput()
        {
            // Arrange
            Order order = null;
            _mockOrderValidator
                .Setup(validator => validator.Validate(It.IsAny<Order>()))
                .Callback((Order o) => order = o)
                .Returns(new ValidationResult(new[] { new ValidationFailure(string.Empty, string.Empty) }));

            // Act
            _target.Create(new Order { Id = orderId });

            // Assert
            Assert.IsNotNull(order);
            Assert.AreEqual(orderId, order.Id);
        }

        [TestMethod]
        public void CreateOrder_ReturnsBadRequestWithErrorMessages_IfValidationFails()
        {
            // Arrange
            _mockOrderValidator
                .Setup(validator => validator.Validate(It.IsAny<Order>()))
                .Returns(new ValidationResult(new[] { new ValidationFailure(string.Empty, Message(1)), new ValidationFailure(string.Empty, Message(2)) }));

            // Act
            var result = _target.Create(new Order()) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.IsTrue(result.Value.ToString().Contains(Message(1)));
            Assert.IsTrue(result.Value.ToString().Contains(Message(2)));
        }

        [TestMethod]
        public void CreateOrder_InvokesMapperWithCorrectInput_IfValidationSucceeds()
        {
            // Arrange
            SetupOrderValidationSuccess();

            Order mapperInput = null;
            _mockOrderMapper
                .Setup(mapper => mapper.Map<Order, OrderModel>(It.IsAny<Order>()))
                .Callback((Order o) => mapperInput = o);

            _mockOrderService
                .Setup(service => service.CreateNewOrder(It.IsAny<OrderModel>()))
                .Returns(new ServiceResponse<int>());

            // Act
            _target.Create(new Order { Id = orderId });

            // Assert
            Assert.IsNotNull(mapperInput);
            Assert.AreEqual(orderId, mapperInput.Id);
        }

        [TestMethod]
        public void CreateOrder_InvokesServiceCreateNewOrderWithMapperOutput()
        {
            // Arrange
            SetupOrderValidationSuccess();

            _mockOrderMapper
                .Setup(mapper => mapper.Map<Order, OrderModel>(It.IsAny<Order>()))
                .Returns(new OrderModel { Id = orderId });

            OrderModel model = null;
            _mockOrderService
                .Setup(service => service.CreateNewOrder(It.IsAny<OrderModel>()))
                .Callback((OrderModel om) => model = om)
                .Returns(new ServiceResponse<int>());

            // Act
            _target.Create(new Order());

            // Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(orderId, model.Id);
        }

        [TestMethod]
        public void CreateOrder_ReturnsCreatedWithNewId_IfServiceResultSuccessful()
        {
            // Arrange
            SetupOrderValidationSuccess();

            _mockOrderService
                .Setup(service => service.CreateNewOrder(It.IsAny<OrderModel>()))
                .Returns(new ServiceResponse<int>(orderId));

            // Act
            var result = _target.Create(new Order()) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.Created, result.StatusCode);
            Assert.AreEqual(orderId, Convert.ToInt32(result.Value));
            Assert.AreEqual($"GetOrder/{orderId}", (result as CreatedResult).Location);
        }

        [TestMethod]
        public void CreateOrder_ReturnsInternalServerErrorWithoutErrorMessages_IfServiceResultIsInternalServerError()
        {
            // Arrange
            SetupOrderValidationSuccess();

            _mockOrderService
                .Setup(service => service.CreateNewOrder(It.IsAny<OrderModel>()))
                .Returns(new ServiceResponse<int>(ServiceError.InternalServerError, Message()));

            // Act
            var result = _target.Create(new Order()) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(result.Value?.ToString()));
        }

        private void SetupOrderValidationSuccess()
        {
            _mockOrderValidator
                .Setup(validator => validator.Validate(It.IsAny<Order>()))
                .Returns(new ValidationResult());
        }

        #endregion CreateOrder Tests

        #region ClearOrder Tests

        [TestMethod]
        public void ClearOrder_InvokesServiceClearOrderWithCorrectArguments_IfLineIdNull()
        {
            // Arrange
            int id = 0;
            _mockOrderService
                .Setup(service => service.ClearOrder(It.IsAny<int>()))
                .Callback((int i) => id = i)
                .Returns(new ServiceResponse());

            // Act
            _target.Clear(orderId, null);

            // Assert
            Assert.AreEqual(orderId, id);
            _mockOrderService.Verify(service => service.DeleteOrderLine(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public void ClearOrder_ReturnsInternalServerErrorWithoutErrorMessages_IfServiceClearOrderResultIsInternalServerError()
        {
            // Arrange
            _mockOrderService
                .Setup(service => service.ClearOrder(It.IsAny<int>()))
                .Returns(new ServiceResponse(ServiceError.InternalServerError, Message()));

            // Act
            var result = _target.Clear(orderId, null) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(result.Value?.ToString()));
        }

        [TestMethod]
        public void ClearOrder_ReturnsOk_IfServiceClearOrderResultSuccessful()
        {
            // Arrange
            _mockOrderService
                .Setup(service => service.ClearOrder(It.IsAny<int>()))
                .Returns(new ServiceResponse());

            // Act
            var result = _target.Clear(orderId, null) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
        }
        
        [TestMethod]
        public void ClearOrder_InvokesServiceDeleteOrderLineWithCorrectArguments_IfLineIdSupplied()
        {
            // Arrange
            int id = 0;
            int inputLineId = 0;
            _mockOrderService
                .Setup(service => service.DeleteOrderLine(It.IsAny<int>(), It.IsAny<int>()))
                .Callback((int i, int j) => { id = i; inputLineId = j; })
                .Returns(new ServiceResponse());

            // Act
            _target.Clear(orderId, lineId);

            // Assert
            Assert.AreEqual(orderId, id);
            Assert.AreEqual(lineId, inputLineId);
            _mockOrderService.Verify(service => service.ClearOrder(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public void ClearOrder_ReturnsInternalServerErrorWithoutErrorMessages_IfServiceDeleteOrderLineResultIsInternalServerError()
        {
            // Arrange
            _mockOrderService
                .Setup(service => service.DeleteOrderLine(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ServiceResponse(ServiceError.InternalServerError, Message()));

            // Act
            var result = _target.Clear(orderId, lineId) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(result.Value?.ToString()));
        }

        [TestMethod]
        public void ClearOrder_ReturnsBadRequestWithErrorMessages_IfServiceDeleteOrderLineResultIsBadRequest()
        {
            // Arrange
            _mockOrderService
                .Setup(service => service.DeleteOrderLine(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ServiceResponse(ServiceError.BadRequest, Message(1), Message(2)));

            // Act
            var result = _target.Clear(orderId, lineId) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.IsTrue(result.Value.ToString().Contains(Message(1)));
            Assert.IsTrue(result.Value.ToString().Contains(Message(2)));
        }

        [TestMethod]
        public void ClearOrder_ReturnsNotFound_IfServiceDeleteOrderLineResultIsNotFound()
        {
            // Arrange
            _mockOrderService
                .Setup(service => service.DeleteOrderLine(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ServiceResponse(ServiceError.NotFound, Message()));

            // Act
            var result = _target.Clear(orderId, lineId) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.NotFound, result.StatusCode);
            Assert.IsTrue(result.Value.ToString().Contains(Message()));
        }

        [TestMethod]
        public void ClearOrder_ReturnsOk_IfServiceDeleteOrderLineResultSuccessful()
        {
            // Arrange
            _mockOrderService
                .Setup(service => service.DeleteOrderLine(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new ServiceResponse());

            // Act
            var result = _target.Clear(orderId, lineId) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
        }

        #endregion ClearOrder Tests

        #region Update Tests

        [TestMethod]
        public void Update_ValidatesInput()
        {
            // Arrange
            OrderLine line = null;
            _mockOrderLineValidator
                .Setup(validator => validator.Validate(It.IsAny<OrderLine>()))
                .Callback((OrderLine l) => line = l)
                .Returns(new ValidationResult(new[] { new ValidationFailure(string.Empty, string.Empty) }));

            // Act
            _target.Update(orderId, new OrderLine { Id = lineId });

            // Assert
            Assert.IsNotNull(line);
            Assert.AreEqual(lineId, line.Id);
        }

        [TestMethod]
        public void Update_ReturnsBadRequestWithErrorMessages_IfValidationFails()
        {
            // Arrange
            _mockOrderLineValidator
                .Setup(validator => validator.Validate(It.IsAny<OrderLine>()))
                .Returns(new ValidationResult(new[] { new ValidationFailure(string.Empty, Message(1)), new ValidationFailure(string.Empty, Message(2)) }));

            // Act
            var result = _target.Update(orderId, new OrderLine()) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.IsTrue(result.Value.ToString().Contains(Message(1)));
            Assert.IsTrue(result.Value.ToString().Contains(Message(2)));
        }

        [TestMethod]
        public void Update_InvokesMapperWithCorrectInput_IfValidationSucceeds()
        {
            // Arrange
            SetupOrderLineValidationSuccess();

            OrderLine mapperInput = null;
            _mockOrderMapper
                .Setup(mapper => mapper.Map<OrderLine, OrderLineModel>(It.IsAny<OrderLine>()))
                .Callback((OrderLine line) => mapperInput = line);

            _mockOrderService
                .Setup(service => service.UpdateOrderLine(It.IsAny<int>(), It.IsAny<OrderLineModel>()))
                .Returns(new ServiceResponse<int>());

            // Act
            _target.Update(orderId, new OrderLine { Id = lineId });

            // Assert
            Assert.IsNotNull(mapperInput);
            Assert.AreEqual(lineId, mapperInput.Id);
        }

        [TestMethod]
        public void Update_InvokesServiceUpdateOrderLineWithMapperOutput()
        {
            // Arrange
            SetupOrderLineValidationSuccess();

            _mockOrderMapper
                .Setup(mapper => mapper.Map<OrderLine, OrderLineModel>(It.IsAny<OrderLine>()))
                .Returns(new OrderLineModel(lineId, new SkuModel(string.Empty, string.Empty)));

            int id = 0;
            OrderLineModel model = null;
            _mockOrderService
                .Setup(service => service.UpdateOrderLine(It.IsAny<int>(), It.IsAny<OrderLineModel>()))
                .Callback((int i, OrderLineModel olm) => { id = i; model = olm; })
                .Returns(new ServiceResponse<int>());

            // Act
            _target.Update(orderId, new OrderLine());

            // Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(lineId, model.Id);
            Assert.AreEqual(orderId, id);
        }

        [TestMethod]
        public void Update_ReturnsInternalServerErrorWithoutErrorMessages_IfServiceUpdateOrderLineResultIsInternalServerError()
        {
            // Arrange
            SetupOrderLineValidationSuccess();

            _mockOrderService
                .Setup(service => service.UpdateOrderLine(It.IsAny<int>(), It.IsAny<OrderLineModel>()))
                .Returns(new ServiceResponse<int>(ServiceError.InternalServerError, Message()));

            // Act
            var result = _target.Update(orderId, new OrderLine()) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(result.Value?.ToString()));
        }

        [TestMethod]
        public void Update_ReturnsBadRequestWithErrorMessages_IfServiceUpdateOrderLineResultIsBadRequest()
        {
            // Arrange
            SetupOrderLineValidationSuccess();

            _mockOrderService
                .Setup(service => service.UpdateOrderLine(It.IsAny<int>(), It.IsAny<OrderLineModel>()))
                .Returns(new ServiceResponse<int>(ServiceError.BadRequest, Message(1), Message(2)));

            // Act
            var result = _target.Update(orderId, new OrderLine()) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.IsTrue(result.Value.ToString().Contains(Message(1)));
            Assert.IsTrue(result.Value.ToString().Contains(Message(2)));
        }

        [TestMethod]
        public void Update_ReturnsNotFound_IfServiceUpdateOrderLineResultIsNotFound()
        {
            // Arrange
            SetupOrderLineValidationSuccess();

            _mockOrderService
                .Setup(service => service.UpdateOrderLine(It.IsAny<int>(), It.IsAny<OrderLineModel>()))
                .Returns(new ServiceResponse<int>(ServiceError.NotFound, Message()));

            // Act
            var result = _target.Update(orderId, new OrderLine()) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.NotFound, result.StatusCode);
            Assert.IsTrue(result.Value.ToString().Contains(Message()));
        }

        [TestMethod]
        public void Update_ReturnsCreatedWithNewId_IfServiceResultSuccessful()
        {
            // Arrange
            SetupOrderLineValidationSuccess();

            _mockOrderService
                .Setup(service => service.UpdateOrderLine(It.IsAny<int>(), It.IsAny<OrderLineModel>()))
                .Returns(new ServiceResponse<int>(lineId));

            // Act
            var result = _target.Update(orderId, new OrderLine()) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.Created, result.StatusCode);
            Assert.AreEqual(lineId, Convert.ToInt32(result.Value));
            Assert.AreEqual(string.Empty, (result as CreatedResult).Location);
        }

        private void SetupOrderLineValidationSuccess()
        {
            _mockOrderLineValidator
                .Setup(validator => validator.Validate(It.IsAny<OrderLine>()))
                .Returns(new ValidationResult());
        }

        #endregion Update Tests
    }
}
