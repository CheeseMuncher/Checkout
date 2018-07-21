using AutoMapper;
using CheckoutApi.Mappers;
using CheckoutOrderService.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace CheckoutApiTests
{
    [TestClass]
    public class MapperTests
    {
        private IMapper _target;

        [TestInitialize]
        public void TestInit()
        {
            _target = new MapperConfiguration(cfg => cfg.AddProfile(new OrderMapper())).CreateMapper();
        }

        [TestMethod]
        public void OrderMapper_MapsPropertiesFromServiceOrderLineModelByName()
        {
            // Arrange
            var serviceModel = new OrderLineModel(2, new SkuModel(string.Empty, string.Empty))
            {
                SortOrder = 3,
                Quantity = 5
            };

            // Act
            var result = _target.Map<OrderLineModel, OrderLine>(serviceModel);

            // Assert
            Assert.AreEqual(serviceModel.Id, result.Id);
            Assert.AreEqual(serviceModel.SortOrder, result.SortOrder);
            Assert.AreEqual(serviceModel.Quantity, result.Quantity);
        }

        [TestMethod]
        public void OrderMapper_MapsSkuPropertiesFromServiceOrderLineModel()
        {
            // Arrange
            var serviceModel = new OrderLineModel(2, new SkuModel("3", "Sku3"));

            // Act
            var result = _target.Map<OrderLineModel, OrderLine>(serviceModel);

            // Assert
            Assert.AreEqual(serviceModel.Sku.Id, result.SkuCode);
            Assert.AreEqual(serviceModel.Sku.DisplayName, result.SkuDisplayName);
        }

        [TestMethod]
        public void OrderMapper_MapsPropertiesFromServiceOrderModelByName()
        {
            // Arrange
            var serviceModel = new OrderModel { Id = 2 };

            // Act
            var result = _target.Map<OrderModel, Order>(serviceModel);

            // Assert
            Assert.AreEqual(serviceModel.Id, result.Id);
        }

        [TestMethod]
        public void OrderMapper_MapsSingleOrderLine()
        {
            // Arrange
            var serviceLineModel = new OrderLineModel(2, new SkuModel(string.Empty, string.Empty));
            var serviceOrderModel = new OrderModel { Lines = new List<OrderLineModel> { serviceLineModel } };

            // Act
            var result = _target.Map<OrderModel, Order>(serviceOrderModel);

            // Assert
            Assert.AreEqual(serviceOrderModel.Lines.Count, result.Lines.Count);
            Assert.AreEqual(serviceLineModel.Id, result.Lines.First().Id);
        }

        [TestMethod]
        public void OrderMapper_MapsMultipleOrderLines()
        {
            // Arrange
            var serviceLineModel1 = new OrderLineModel(1, new SkuModel(string.Empty, string.Empty));
            var serviceLineModel2 = new OrderLineModel(2, new SkuModel(string.Empty, string.Empty));
            var serviceOrderModel = new OrderModel { Lines = new List<OrderLineModel> { serviceLineModel1, serviceLineModel2 } };

            // Act
            var result = _target.Map<OrderModel, Order>(serviceOrderModel);

            // Assert
            Assert.AreEqual(serviceOrderModel.Lines.Count, result.Lines.Count);
            Assert.IsTrue(result.Lines.Any(line => line.Id == serviceLineModel1.Id));
            Assert.IsTrue(result.Lines.Any(line => line.Id == serviceLineModel2.Id));
        }
    }
}
