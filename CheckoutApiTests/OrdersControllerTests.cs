using AutoMapper;
using CheckoutApi.Controllers;
using CheckoutOrderService;
using CheckoutOrderService.Common;
using CheckoutOrderService.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
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

        private const int orderId = 11;

        [TestInitialize]
        public void TestInit()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockOrderMapper = new Mock<IMapper>();
            _mockOrderValidator = new Mock<IValidator<Order>>();

            _target = new OrdersController(_mockOrderService.Object, _mockOrderMapper.Object, _mockOrderValidator.Object);
        }

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
            var message1 = "Error Message 1";
            var message2 = "Error Message 2";
            _mockOrderService
                .Setup(service => service.GetOrder(It.IsAny<int>()))
                .Returns(new ServiceResponse<OrderModel>(ServiceError.NotFound, message1, message2));

            // Act
            var result = _target.Get(orderId).Result as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.NotFound, result.StatusCode);
            Assert.IsTrue(result.Value.ToString().Contains(message1));
            Assert.IsTrue(result.Value.ToString().Contains(message2));
        }

        [TestMethod]
        public void GetOrder_ReturnsInternalServerErrorWithoutErrorMessages_IfServiceResultIsInternalServerError()
        {
            // Arrange
            var message = "Error Message";
            _mockOrderService
                .Setup(service => service.GetOrder(It.IsAny<int>()))
                .Returns(new ServiceResponse<OrderModel>(ServiceError.InternalServerError, message));

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
            var message1 = "Error Message 1";
            var message2 = "Error Message 2";
            _mockOrderValidator
                .Setup(validator => validator.Validate(It.IsAny<Order>()))
                .Returns(new ValidationResult(new[] { new ValidationFailure(string.Empty, message1), new ValidationFailure(string.Empty, message2) }));

            // Act
            var result = _target.Create(new Order()) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.IsTrue(result.Value.ToString().Contains(message1));
            Assert.IsTrue(result.Value.ToString().Contains(message2));
        }

        [TestMethod]
        public void CreateOrder_InvokesMapperWithCorrectInput_IfValidationSucceeds()
        {
            // Arrange
            _mockOrderValidator
                .Setup(validator => validator.Validate(It.IsAny<Order>()))
                .Returns(new ValidationResult());

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
            _mockOrderValidator
                .Setup(validator => validator.Validate(It.IsAny<Order>()))
                .Returns(new ValidationResult());

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
            _mockOrderValidator
                .Setup(validator => validator.Validate(It.IsAny<Order>()))
                .Returns(new ValidationResult());

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
            _mockOrderValidator
                .Setup(validator => validator.Validate(It.IsAny<Order>()))
                .Returns(new ValidationResult());

            var message = "Error Message";
            _mockOrderService
                .Setup(service => service.CreateNewOrder(It.IsAny<OrderModel>()))
                .Returns(new ServiceResponse<int>(ServiceError.InternalServerError, message));

            // Act
            var result = _target.Create(new Order()) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.IsTrue(string.IsNullOrEmpty(result.Value?.ToString()));
        }
    }
}
