using AutoMapper;
using CheckoutApi.Mappers;
using CheckoutApi.Models;
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
            _target = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new OrderMapper());
                cfg.AddProfile(new SkuMapper());
            }).CreateMapper();
        }

        [TestMethod]
        public void OrderMapper_Outbound_MapsPropertiesFromServiceOrderLineModelByName()
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
        public void OrderMapper_Outbound_MapsSkuPropertiesFromServiceOrderLineModel()
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
        public void OrderMapper_Outbound_MapsPropertiesFromServiceOrderModelByName()
        {
            // Arrange
            var serviceModel = new OrderModel(2);

            // Act
            var result = _target.Map<OrderModel, Order>(serviceModel);

            // Assert
            Assert.AreEqual(serviceModel.Id, result.Id);
        }

        [TestMethod]
        public void OrderMapper_Outbound_MapsSingleOrderLine()
        {
            // Arrange
            var serviceLineModel = new OrderLineModel(2, new SkuModel(string.Empty, string.Empty));
            var serviceOrderModel = new OrderModel(0) { Lines = new List<OrderLineModel> { serviceLineModel } };

            // Act
            var result = _target.Map<OrderModel, Order>(serviceOrderModel);

            // Assert
            Assert.AreEqual(serviceOrderModel.Lines.Count, result.Lines.Count);
            Assert.AreEqual(serviceLineModel.Id, result.Lines.First().Id);
        }

        [TestMethod]
        public void OrderMapper_Outbound_MapsMultipleOrderLines()
        {
            // Arrange
            var serviceLineModel1 = new OrderLineModel(1, new SkuModel(string.Empty, string.Empty));
            var serviceLineModel2 = new OrderLineModel(2, new SkuModel(string.Empty, string.Empty));
            var serviceOrderModel = new OrderModel(0) { Lines = new List<OrderLineModel> { serviceLineModel1, serviceLineModel2 } };

            // Act
            var result = _target.Map<OrderModel, Order>(serviceOrderModel);

            // Assert
            Assert.AreEqual(serviceOrderModel.Lines.Count, result.Lines.Count);
            Assert.IsTrue(result.Lines.Any(line => line.Id == serviceLineModel1.Id));
            Assert.IsTrue(result.Lines.Any(line => line.Id == serviceLineModel2.Id));
        }

        [TestMethod]
        public void OrderMapper_Inbound_MapsPropertiesToServiceOrderLineModelByName()
        {
            // Arrange
            var apiModel = new OrderLine
            {
                SortOrder = 2,
                Quantity = 3,
            };

            // Act
            var result = _target.Map<OrderLine, OrderLineModel>(apiModel);

            // Assert
            Assert.AreEqual(apiModel.SortOrder, result.SortOrder);
            Assert.AreEqual(apiModel.Quantity, result.Quantity);
        }

        [TestMethod]
        public void OrderMapper_Inbound_MapsImmutablePropertiesToServiceOrderLineModel()
        {
            // Arrange
            var apiModel = new OrderLine
            {
                Id = 2,
                SkuCode = "SkuCode",
                SkuDisplayName = "Sku Name"
            };

            // Act
            var result = _target.Map<OrderLine, OrderLineModel>(apiModel);

            // Assert
            Assert.AreEqual(apiModel.Id, result.Id);
            Assert.AreEqual(apiModel.SkuCode, result.Sku.Id);
            Assert.AreEqual(apiModel.SkuDisplayName, result.Sku.DisplayName);
        }

        [TestMethod]
        public void OrderMapper_Inbound_MapsPropertiesToServiceOrderModelByName()
        {
            // Arrange
            var apiModel = new Order { Id = 2 };

            // Act
            var result = _target.Map<Order, OrderModel>(apiModel);

            // Assert
            Assert.AreEqual(apiModel.Id, result.Id);
        }

        [TestMethod]
        public void OrderMapper_Inbound_MapsSingleOrderLine()
        {
            // Arrange
            var apiLineModel = new OrderLine { Id = 2 };
            var apiOrderModel = new Order { Lines = new List<OrderLine> { apiLineModel } };

            // Act
            var result = _target.Map<Order, OrderModel>(apiOrderModel);

            // Assert
            Assert.AreEqual(apiOrderModel.Lines.Count, result.Lines.Count);
            Assert.AreEqual(apiLineModel.Id, result.Lines.First().Id);
        }

        [TestMethod]
        public void OrderMapper_Inbound_MapsMultipleOrderLines()
        {
            // Arrange
            var apiLineModel1 = new OrderLine { Id = 1 };
            var apiLineModel2 = new OrderLine { Id = 2 };
            var serviceOrderModel = new Order { Lines = new List<OrderLine> { apiLineModel1, apiLineModel2 } };

            // Act
            var result = _target.Map<Order, OrderModel>(serviceOrderModel);

            // Assert
            Assert.AreEqual(serviceOrderModel.Lines.Count, result.Lines.Count);
            Assert.IsTrue(result.Lines.Any(line => line.Id == apiLineModel1.Id));
            Assert.IsTrue(result.Lines.Any(line => line.Id == apiLineModel2.Id));
        }

        [TestMethod]
        public void SkuMapper_Outbound_MapsPropertiesFromServiceSkuModelByName()
        {
            // Arrange
            var serviceModel = new SkuModel(string.Empty, "Test1");

            // Act
            var result = _target.Map<SkuModel, Sku>(serviceModel);

            // Assert
            Assert.AreEqual(serviceModel.DisplayName, result.DisplayName);
        }

        [TestMethod]
        public void SkuMapper_Outbound_MapsCodeFromServiceSkuModel()
        {
            // Arrange
            var serviceModel = new SkuModel("Test1", string.Empty);

            // Act
            var result = _target.Map<SkuModel, Sku>(serviceModel);

            // Assert
            Assert.AreEqual(serviceModel.Id, result.Code);
        }
    }
}
