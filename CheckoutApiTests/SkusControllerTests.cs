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
    public class SkusControllerTests
    {
        private SkusController _target;

        private Mock<IOrderService> _mockOrderService;
        private Mock<IMapper> _mockOrderMapper;

        private const int orderId = 11;
        private const int lineId = 13;

        private string Message() => "Error Message";

        [TestInitialize]
        public void TestInit()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockOrderMapper = new Mock<IMapper>();

            _target = new SkusController(_mockOrderService.Object, _mockOrderMapper.Object);
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
    }
}
