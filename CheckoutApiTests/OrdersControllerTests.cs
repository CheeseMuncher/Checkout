using AutoMapper;
using CheckoutApi.Controllers;
using CheckoutOrderService;
using CheckoutOrderService.Common;
using CheckoutOrderService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net;

namespace CheckoutApiTests
{
    [TestClass]
    public class OrdersControllerTests
    {
        private OrdersController _target;

        private Mock<IOrderService> _mockOrderService;
        private Mock<IMapper> _mockOrderMapper;

        private const int orderId = 11;

        [TestInitialize]
        public void TestInit()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockOrderMapper = new Mock<IMapper>();

            _target = new OrdersController(_mockOrderService.Object, _mockOrderMapper.Object);
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
        public void GetOrder_ReturnsInternalServerExceptionWithoutErrorMessages_IfServiceResultIsInternalServerException()
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
    }
}
