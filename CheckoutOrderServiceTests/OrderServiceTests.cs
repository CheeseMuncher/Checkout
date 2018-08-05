using CheckoutOrderService;
using CheckoutOrderService.Common;
using CheckoutOrderService.Dependencies;
using CheckoutOrderService.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CheckoutOrderServiceTests
{
    [TestClass]
    public class OrderServiceTests
    {
        #region Private Members

        private IOrderService _target;

        private Mock<IRepository> _mockRepository;
        private Mock<ILogger> _mockLogger;

        private const int orderId = 11;
        private const int orderLineId = 13;

        private const string exceptionMessage = "Exception Message";
        private const string newOrderError = "Error creating new Order";
        private const string orderNotFoundError = "Order not found";
        private string fetchingOrderError = $"Error fetching order with id {orderId}";
        private string savingOrderError = $"Error saving order with id {orderId}";
        private string deletingOrderLineError = $"Error deleting line with {orderLineId} from order with id {orderId}";
        private string clearingOrderLineError = $"Error clearing order with id {orderId}";

        private SkuModel GetSkuModel(int suffix) => new SkuModel($"SkuCode{suffix}", $"Product {suffix}");

        private OrderLineModel GetOrderLine(int id) => new OrderLineModel(id, GetSkuModel(id)) { Quantity = 1, SortOrder = 100 };

        private OrderLineModel GetOrderLine(int id1, int id2) => new OrderLineModel(id1, GetSkuModel(id2)) { Quantity = 1, SortOrder = 100 };

        #endregion Private Members

        [TestInitialize]
        public void TestInit()
        {
            _mockRepository = new Mock<IRepository>();
            _mockLogger = new Mock<ILogger>();

            _target = new OrderService(_mockRepository.Object, _mockLogger.Object);
        }

        #region GetSkus

        [TestMethod]
        public void GetSkus_InvokesRepoGetWithCorrectArguments()
        {
            // Arrange
            Expression<Func<SkuModel, bool>> predicate = null;
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<SkuModel, bool>>>())).Callback((Expression<Func<SkuModel, bool>> exp) => predicate = exp);

            // Act
            _target.GetSkus();

            // Assert
            Assert.IsNotNull(predicate);
            var func = predicate.Compile();
            Assert.IsTrue(func(GetSkuModel(1)));
        }

        [TestMethod]
        public void GetSkus_ReturnsRepoResult()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<SkuModel, bool>>>())).Returns(new List<SkuModel> { GetSkuModel(1), GetSkuModel(2) } );

            // Act
            var result = _target.GetSkus();

            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(2, result.Data.Count());
            Assert.AreEqual(GetSkuModel(1).DisplayName, result.Data.First(sku => sku.Id == GetSkuModel(1).Id).DisplayName);
            Assert.AreEqual(GetSkuModel(2).DisplayName, result.Data.First(sku => sku.Id == GetSkuModel(2).Id).DisplayName);
        }

        [TestMethod]
        public void GetSkus_LogsError_IfRepoGetThrows()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<SkuModel, bool>>>())).Throws(new Exception(exceptionMessage));

            string loggedMessage = null;
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>())).Callback((string str) => loggedMessage = str);

            // Act
            var result = _target.GetSkus();

            // Assert
            Assert.IsNotNull(loggedMessage);
            Assert.IsTrue(loggedMessage.StartsWith($"{nameof(OrderService)}.{nameof(OrderService.GetSkus)}"));
            Assert.IsTrue(loggedMessage.Contains("Error fetching SKUs"));
            Assert.IsTrue(loggedMessage.Contains(exceptionMessage));
        }

        [TestMethod]
        public void GetSkus_ReturnsInternalServerError_IfExceptionThrown()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<SkuModel, bool>>>())).Throws(new Exception());

            // Act
            var result = _target.GetSkus();

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.InternalServerError, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains("Error fetching SKUs"));
        }

        [TestMethod]
        public void GetSkus_RepoGetReturnsDummyData()
        {
            // Arrange
            _mockRepository
                .Setup(repo => repo.Get(It.IsAny<Expression<Func<SkuModel, bool>>>()))
                .Returns((Expression<Func<SkuModel, bool>> predicate) => new Repository().Get(predicate));

            IEnumerable<SkuModel> result = null;

            // Act
            try
            {
                result = _target.GetSkus().Data;
            }

            // Assert
            catch (Exception e)
            {
                Assert.Fail("Repository was not supposed to throw, consider adding a new test");
            }
            finally
            {
                Assert.AreEqual(4, result.Count(), "Hard-coded values have changed, consider replacing this test with new ones");
            }
        }

        #endregion GetSkus

        #region CreateNewOrder

        [TestMethod]
        public void CreateNewOrder_SavesNewOrder()
        {
            // Arrange
            OrderModel savePayload = null;
            _mockRepository.Setup(repo => repo.Save(It.IsAny<OrderModel>())).Callback((OrderModel order) => savePayload = order);

            // Act
            _target.CreateNewOrder(new OrderModel { Id = orderId });

            // Assert
            Assert.IsNotNull(savePayload);
            Assert.AreEqual(orderId, savePayload.Id);
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Once);
        }

        [TestMethod]
        public void CreateNewOrder_SavesNewOrder_IfNullSupplied()
        {
            // Arrange
            OrderModel savePayload = null;
            _mockRepository.Setup(repo => repo.Save(It.IsAny<OrderModel>())).Callback((OrderModel order) => savePayload = order);

            // Act
            _target.CreateNewOrder(null);

            // Assert
            Assert.IsNotNull(savePayload);
        }

        [TestMethod]
        public void CreateNewOrder_ReturnsRepoResponseId()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Save(It.IsAny<OrderModel>())).Returns(new OrderModel { Id = orderId });

            // Act
            var result = _target.CreateNewOrder(new OrderModel());

            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(orderId, result.Data);
        }

        [TestMethod]
        public void CreateNewOrder_ReturnsInternalServerError_IfExceptionThrown()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Save(It.IsAny<OrderModel>())).Throws(new Exception());

            // Act
            var result = _target.CreateNewOrder(new OrderModel());

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.InternalServerError, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(newOrderError));
        }

        [TestMethod]
        public void CreateNewOrder_LogsError_IfExceptionThrown()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Save(It.IsAny<OrderModel>())).Throws(new Exception(exceptionMessage));

            string loggedMessage = null;
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>())).Callback((string str) => loggedMessage = str);

            // Act
            var result = _target.CreateNewOrder(new OrderModel());

            // Assert
            Assert.IsNotNull(loggedMessage);
            Assert.IsTrue(loggedMessage.StartsWith($"{nameof(OrderService)}.{nameof(OrderService.CreateNewOrder)}"));
            Assert.IsTrue(loggedMessage.Contains(newOrderError));
            Assert.IsTrue(loggedMessage.Contains(exceptionMessage));
        }

        #endregion CreateNewOrder

        #region GetOrder

        [TestMethod]
        public void GetOrder_InvokesRepoGetWithCorrectArguments()
        {
            // Arrange
            Expression<Func<OrderModel, bool>> predicate = null;
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Callback((Expression<Func<OrderModel, bool>> exp) => predicate = exp);

            // Act
            _target.GetOrder(orderId);

            // Assert
            Assert.IsNotNull(predicate);

            var matchingOrder = new OrderModel { Id = orderId };
            var otherOrder = new OrderModel { Id = orderId + 1 };

            var func = predicate.Compile();
            Assert.IsTrue(func(matchingOrder));
            Assert.IsFalse(func(otherOrder));
        }

        [TestMethod]
        public void GetOrder_ReturnsNotFound_IfRepoGetResultIsNull()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(() => null);

            // Act
            var result = _target.GetOrder(default(int));

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.NotFound, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(orderNotFoundError));
        }

        [TestMethod]
        public void GetOrder_ReturnsNotFound_IfRepoGetResultEmpty()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(() => new List<OrderModel>());

            // Act
            var result = _target.GetOrder(default(int));

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.NotFound, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(orderNotFoundError));
        }

        [TestMethod]
        public void GetOrder_ReturnsInternalServerError_IfRepoGetReturnsMultipleResults()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>()))
                .Returns(() => new List<OrderModel> { new OrderModel { Id = orderId }, new OrderModel { Id = orderId } });

            // Act
            var result = _target.GetOrder(orderId);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.InternalServerError, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(fetchingOrderError));
        }

        [TestMethod]
        public void GetOrder_LogsError_IfRepoGetReturnsMultipleResults()
        {
            // Arrange
            _mockRepository
                .Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>()))
                .Returns(() => new List<OrderModel> { new OrderModel { Id = orderId }, new OrderModel { Id = orderId } });

            string loggedMessage = null;
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>())).Callback((string str) => loggedMessage = str);

            // Act
            var result = _target.GetOrder(orderId);

            // Assert
            Assert.IsNotNull(loggedMessage);
            Assert.IsTrue(loggedMessage.StartsWith($"{nameof(OrderService)}.{nameof(OrderService.GetOrder)}"));
            Assert.IsTrue(loggedMessage.Contains(fetchingOrderError));
            Assert.IsTrue(loggedMessage.Contains("duplicate matches found"));
            Assert.IsTrue(loggedMessage.Contains("2"));
        }

        [TestMethod]
        public void GetOrder_ReturnsRepoResult_IfRepoReturnsSingleResult()
        {
            // Arrange
            _mockRepository
                .Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>()))
                .Returns(() => new List<OrderModel> { new OrderModel { Id = orderId } });

            // Act
            var result = _target.GetOrder(orderId);

            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(orderId, result.Data.Id);
        }

        [TestMethod]
        public void GetOrder_ReturnsInternalServerError_IfExceptionThrown()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Throws(new Exception());

            // Act
            var result = _target.GetOrder(orderId);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.InternalServerError, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(fetchingOrderError));
        }

        [TestMethod]
        public void GetOrder_LogsError_IfExceptionThrown()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Throws(new Exception(exceptionMessage));

            string loggedMessage = null;
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>())).Callback((string str) => loggedMessage = str);

            // Act
            var result = _target.GetOrder(orderId);

            // Assert
            Assert.IsNotNull(loggedMessage);
            Assert.IsTrue(loggedMessage.StartsWith($"{nameof(OrderService)}.{nameof(OrderService.GetOrder)}"));
            Assert.IsTrue(loggedMessage.Contains(fetchingOrderError));
            Assert.IsTrue(loggedMessage.Contains(exceptionMessage));
        }

        #endregion GetOrder

        #region ClearOrder

        [TestMethod]
        public void ClearOrder_FetchesCorrespondingOrder()
        {
            // Arrange
            Expression<Func<OrderModel, bool>> predicate = null;
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Callback((Expression<Func<OrderModel, bool>> exp) => predicate = exp);

            // Act
            _target.ClearOrder(orderId);

            // Assert
            Assert.IsNotNull(predicate);

            var matchingOrder = new OrderModel { Id = orderId };
            var otherOrder = new OrderModel { Id = orderId + 1 };

            var func = predicate.Compile();
            Assert.IsTrue(func(matchingOrder));
            Assert.IsFalse(func(otherOrder));
        }

        [TestMethod]
        public void ClearOrder_ReturnsNotFound_IfOrderNotFound()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(() => null);

            // Act
            var result = _target.ClearOrder(orderId);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.NotFound, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(orderNotFoundError));
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Never);
        }

        [TestMethod]
        public void ClearOrder_ReturnsInternalServerError_IfOrderGetFails()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Throws(new Exception());

            // Act
            var result = _target.ClearOrder(orderId);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.InternalServerError, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(fetchingOrderError));
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Never);
        }

        [TestMethod]
        public void ClearOrder_ReturnsSuccess_IfOrderLinesNull()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(() => new List<OrderModel> { repoOrder });

            // Act
            var result = _target.ClearOrder(orderId);

            // Assert
            Assert.IsTrue(result.IsSuccessful);
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Never);
        }

        [TestMethod]
        public void ClearOrder_ReturnsSuccess_IfOrderLinesEmpty()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel>() };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(() => new List<OrderModel> { repoOrder });

            // Act
            var result = _target.ClearOrder(orderId);

            // Assert
            Assert.IsTrue(result.IsSuccessful);
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Never);
        }

        [TestMethod]
        public void ClearOrder_SavesOrderWithoutLines_IfOrderContainsLines()
        {
            // Arrange
            var line1 = GetOrderLine(orderLineId);
            var line2 = GetOrderLine(orderLineId + 1);
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { line1, line2 } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            OrderModel savePayload = null;
            _mockRepository.Setup(repo => repo.Save(It.IsAny<OrderModel>())).Callback((OrderModel order) => savePayload = order);

            // Act
            _target.ClearOrder(orderId);

            // Assert
            Assert.IsNotNull(savePayload);
            Assert.AreEqual(0, savePayload.Lines.Count());
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Once);
        }

        [TestMethod]
        public void ClearOrder_ReturnsSuccess_AfterOrderLinesDeleted()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            // Act
            var result = _target.ClearOrder(orderId);

            // Assert
            Assert.IsTrue(result.IsSuccessful);
        }

        [TestMethod]
        public void ClearOrder_ReturnsInternalServerError_IfExceptionThrown()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            _mockRepository.Setup(repo => repo.Save(It.IsAny<OrderModel>())).Throws(new Exception());

            // Act
            var result = _target.ClearOrder(orderId);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.InternalServerError, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(clearingOrderLineError));
        }

        [TestMethod]
        public void ClearOrder_LogsError_IfExceptionThrown()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            _mockRepository.Setup(repo => repo.Save(It.IsAny<OrderModel>())).Throws(new Exception(exceptionMessage));

            string loggedMessage = null;
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>())).Callback((string str) => loggedMessage = str);

            // Act
            _target.ClearOrder(orderId);

            // Assert
            Assert.IsNotNull(loggedMessage);
            Assert.IsTrue(loggedMessage.StartsWith($"{nameof(OrderService)}.{nameof(OrderService.ClearOrder)}"));
            Assert.IsTrue(loggedMessage.Contains(clearingOrderLineError));
            Assert.IsTrue(loggedMessage.Contains(exceptionMessage));
        }

        #endregion ClearOrder

        #region UpdateOrderLine

        [TestMethod]
        public void UpdateOrderLine_FetchesCorrespondingOrder()
        {
            // Arrange
            Expression<Func<OrderModel, bool>> predicate = null;
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Callback((Expression<Func<OrderModel, bool>> exp) => predicate = exp);

            // Act
            _target.UpdateOrderLine(orderId, GetOrderLine(orderLineId));

            // Assert
            Assert.IsNotNull(predicate);

            var matchingOrder = new OrderModel { Id = orderId };
            var otherOrder = new OrderModel { Id = orderId + 1 };

            var func = predicate.Compile();
            Assert.IsTrue(func(matchingOrder));
            Assert.IsFalse(func(otherOrder));
        }

        [TestMethod]
        public void UpdateOrderLine_ReturnsNotFound_IfOrderNotFound()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(() => null);

            // Act
            var result = _target.UpdateOrderLine(orderId, GetOrderLine(orderLineId));

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.NotFound, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(orderNotFoundError));
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Never);
        }

        [TestMethod]
        public void UpdateOrderLine_ReturnsInternalServerError_IfOrderGetFails()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Throws(new Exception());

            // Act
            var result = _target.UpdateOrderLine(orderId, GetOrderLine(orderLineId));

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.InternalServerError, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(fetchingOrderError));
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Never);
        }

        [TestMethod]
        public void UpdateOrderLine_ReturnsInternalServerError_IfOrderHasMultipleMatchingLines()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId), GetOrderLine(orderLineId) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            // Act
            var result = _target.UpdateOrderLine(orderId, GetOrderLine(orderLineId));

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.InternalServerError, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(savingOrderError));
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Never);
        }

        [TestMethod]
        public void UpdateOrderLine_LogsError_IfOrderHasMultipleMatchingLines()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId), GetOrderLine(orderLineId), GetOrderLine(orderLineId) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            string loggedMessage = null;
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>())).Callback((string str) => loggedMessage = str);

            // Act
            var result = _target.UpdateOrderLine(orderId, GetOrderLine(orderLineId));

            // Assert
            Assert.IsNotNull(loggedMessage);
            Assert.IsTrue(loggedMessage.StartsWith($"{nameof(OrderService)}.{nameof(OrderService.UpdateOrderLine)}"));
            Assert.IsTrue(loggedMessage.Contains(savingOrderError));
            Assert.IsTrue(loggedMessage.Contains($"duplicate line matches found for order line id {orderLineId}"));
            Assert.IsTrue(loggedMessage.Contains("3"));
        }

        [TestMethod]
        public void UpdateOrderLine_UpdatesCorrespondingOrderLine_IfItExists()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            OrderModel savePayload = null;
            _mockRepository.Setup(repo => repo.Save(It.IsAny<OrderModel>())).Callback((OrderModel order) => savePayload = order);

            var newQuantity = 2;
            var updatedOrderLine = GetOrderLine(orderLineId);
            updatedOrderLine.Quantity = newQuantity;

            // Act
            _target.UpdateOrderLine(orderId, updatedOrderLine);

            // Assert
            Assert.IsNotNull(savePayload);
            Assert.AreEqual(newQuantity, savePayload.Lines.First(line => line.Id == orderLineId).Quantity);
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Once);
        }

        [TestMethod]
        public void UpdateOrderLine_ReturnsInputLineId_IfItExists()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            // Act
            var result = _target.UpdateOrderLine(orderId, GetOrderLine(orderLineId));

            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(orderLineId, result.Data);
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Once);
        }

        [TestMethod]
        public void UpdateOrderLine_InitialisesOrderLines_IfNull()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            // Act
            _target.UpdateOrderLine(orderId, GetOrderLine(orderLineId));

            // Assert
            // Exception thown before test implemented
        }

        [TestMethod]
        public void UpdateOrderLine_ReturnsBadRequest_IfOrderAlreadyHasLineWithMatchingSku()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            var newLine = GetOrderLine(orderLineId + 1, orderLineId);

            // Act
            var result = _target.UpdateOrderLine(orderId, newLine);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.BadRequest, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains($"Order already contains a line with Sku Code {newLine.Sku.Id}"));
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Never);
        }

        [TestMethod]
        public void UpdateOrderLine_InvokesRepoGetWithCorrectArguments_IfLineIsNew()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            Expression<Func<SkuModel, bool>> predicate = null;
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<SkuModel, bool>>>())).Callback((Expression<Func<SkuModel, bool>> exp) => predicate = exp);

            var newOrderLine = GetOrderLine(orderLineId);

            // Act
            _target.UpdateOrderLine(orderId, newOrderLine);

            // Assert
            Assert.IsNotNull(predicate);

            var matchingSku = GetSkuModel(orderLineId);
            var otherSku = GetSkuModel(orderLineId + 1);

            var func = predicate.Compile();
            Assert.IsTrue(func(matchingSku));
            Assert.IsFalse(func(otherSku));
        }

        [TestMethod]
        public void UpdateOrderLine_ReturnsBadRequest_IfRepoGetResultIsNull()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<SkuModel, bool>>>())).Returns(() => null);

            var newLine = GetOrderLine(orderLineId + 1, orderLineId);

            // Act
            var result = _target.UpdateOrderLine(orderId, newLine);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.BadRequest, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains($"{savingOrderError}, sku with code {newLine.Sku.Id} not found"));
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Never);
        }

        [TestMethod]
        public void UpdateOrderLine_ReturnsBadRequest_IfRepoGetResultIsEmpty()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<SkuModel, bool>>>())).Returns(new List<SkuModel>());

            var newLine = GetOrderLine(orderLineId + 1, orderLineId);

            // Act
            var result = _target.UpdateOrderLine(orderId, newLine);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.BadRequest, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains($"{savingOrderError}, sku with code {newLine.Sku.Id} not found"));
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Never);
        }

        [TestMethod]
        public void UpdateOrderLine_ReturnsInternalServerError_IfRepoGetReturnsMultipleSkuResults()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            var skuModel = GetSkuModel(orderLineId);
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<SkuModel, bool>>>())).Returns(new List<SkuModel> { skuModel, skuModel });

            // Act
            var result = _target.UpdateOrderLine(orderId, GetOrderLine(orderLineId));

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.InternalServerError, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(savingOrderError));
        }

        [TestMethod]
        public void UpdateOrderLine_LogsError_IfRepoGetReturnsMultipleSkuResults()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            var skuModel = GetSkuModel(orderLineId);
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<SkuModel, bool>>>())).Returns(new List<SkuModel> { skuModel, skuModel });

            string loggedMessage = null;
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>())).Callback((string str) => loggedMessage = str);

            // Act
            _target.UpdateOrderLine(orderId, GetOrderLine(orderLineId));

            // Assert
            Assert.IsNotNull(loggedMessage);
            Assert.IsTrue(loggedMessage.StartsWith($"{nameof(OrderService)}.{nameof(OrderService.UpdateOrderLine)}"));
            Assert.IsTrue(loggedMessage.Contains(savingOrderError));
            Assert.IsTrue(loggedMessage.Contains($"duplicate matches found for sku id {skuModel.Id}"));
            Assert.IsTrue(loggedMessage.Contains("2"));
        }

        [TestMethod]
        public void UpdateOrderLine_SavesOrder_IfOrderLineIsNewAndValid()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<SkuModel, bool>>>())).Returns(new List<SkuModel> { GetSkuModel(orderLineId) });

            OrderModel savePayload = null;
            _mockRepository
                .Setup(repo => repo.Save(It.IsAny<OrderModel>()))
                .Callback((OrderModel order) => savePayload = order)
                .Returns(new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId + 1) } });

            var newLine = GetOrderLine(orderLineId + 1);

            // Act
            var result = _target.UpdateOrderLine(orderId, newLine);

            // Assert
            Assert.IsNotNull(savePayload);
            Assert.AreEqual(1, savePayload.Lines.Count());
            Assert.AreEqual(newLine.Sku.Id, savePayload.Lines.First().Sku.Id);
            Assert.AreEqual(newLine.Quantity, savePayload.Lines.First().Quantity);
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Once);
        }

        [TestMethod]
        public void UpdateOrderLine_ReturnsInputRepoSaveLineId_IfOrderLineIsNewAndValid()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<SkuModel, bool>>>())).Returns(new List<SkuModel> { GetSkuModel(orderLineId + 1) });

            var updatedRepoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId), GetOrderLine(orderLineId + 1) } };
            _mockRepository.Setup(repo => repo.Save(It.IsAny<OrderModel>())).Returns(updatedRepoOrder);

            // Act
            var result = _target.UpdateOrderLine(orderId, GetOrderLine(default(int), orderLineId + 1)); // line id zero, sku id matches new line in updatedRepoOrder

            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(orderLineId + 1, result.Data);
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Once);
        }

        [TestMethod]
        public void UpdateOrderLine_ReturnsInternalServerError_IfExceptionThrown()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<SkuModel, bool>>>())).Throws(new Exception());

            // Act
            var result = _target.UpdateOrderLine(orderId, GetOrderLine(default(int)));

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.InternalServerError, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(savingOrderError));
        }

        [TestMethod]
        public void UpdateOrderLine_LogsError_IfExceptionThrown()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<SkuModel, bool>>>())).Throws(new Exception(exceptionMessage));

            string loggedMessage = null;
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>())).Callback((string str) => loggedMessage = str);

            // Act
            _target.UpdateOrderLine(orderId, GetOrderLine(default(int)));

            // Assert
            Assert.IsNotNull(loggedMessage);
            Assert.IsTrue(loggedMessage.StartsWith($"{nameof(OrderService)}.{nameof(OrderService.UpdateOrderLine)}"));
            Assert.IsTrue(loggedMessage.Contains(savingOrderError));
            Assert.IsTrue(loggedMessage.Contains(exceptionMessage));
        }

        #endregion UpdateOrderLine

        #region DeleteOrderLine
        
        [TestMethod]
        public void DeleteOrderLine_FetchesCorrespondingOrder()
        {
            // Arrange
            Expression<Func<OrderModel, bool>> predicate = null;
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Callback((Expression<Func<OrderModel, bool>> exp) => predicate = exp);

            // Act
            _target.DeleteOrderLine(orderId, orderLineId);

            // Assert
            Assert.IsNotNull(predicate);

            var matchingOrder = new OrderModel { Id = orderId };
            var otherOrder = new OrderModel { Id = orderId + 1 };

            var func = predicate.Compile();
            Assert.IsTrue(func(matchingOrder));
            Assert.IsFalse(func(otherOrder));
        }

        [TestMethod]
        public void DeleteOrderLine_ReturnsNotFound_IfOrderNotFound()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(() => null);

            // Act
            var result = _target.DeleteOrderLine(orderId, orderLineId);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.NotFound, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(orderNotFoundError));
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Never);
        }

        [TestMethod]
        public void DeleteOrderLine_ReturnsInternalServerError_IfOrderGetFails()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Throws(new Exception());

            // Act
            var result = _target.DeleteOrderLine(orderId, orderLineId);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.InternalServerError, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(fetchingOrderError));
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Never);
        }

        [TestMethod]
        public void DeleteOrderLine_ReturnsBadRequest_IfOrderLinesNull()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(() => new List<OrderModel> { repoOrder });

            // Act
            var result = _target.DeleteOrderLine(orderId, orderLineId);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.BadRequest, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains($"Error removing line from order with id {orderId}, order does not include line with id {orderLineId}"));
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Never);
        }

        [TestMethod]
        public void DeleteOrderLine_ReturnsBadRequest_IfOrderDoesNotContainLine()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId + 1) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(() => new List<OrderModel> { repoOrder });

            // Act
            var result = _target.DeleteOrderLine(orderId, orderLineId);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.BadRequest, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains($"Error removing line from order with id {orderId}, order does not include line with id {orderLineId}"));
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Never);
        }

        [TestMethod]
        public void DeleteOrderLine_SavesOrderWithoutMatchingLine_IfOrderContainsMatchingLine()
        {
            // Arrange
            var lineToDelete = GetOrderLine(orderLineId);
            var otherOrderLine = GetOrderLine(orderLineId + 1);
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { lineToDelete, otherOrderLine } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            OrderModel savePayload = null;
            _mockRepository.Setup(repo => repo.Save(It.IsAny<OrderModel>())).Callback((OrderModel order) => savePayload = order);

            // Act
            _target.DeleteOrderLine(orderId, orderLineId);

            // Assert
            Assert.IsNotNull(savePayload);
            Assert.AreEqual(1, savePayload.Lines.Count());
            Assert.AreEqual(orderLineId + 1, savePayload.Lines.First().Id);
            _mockRepository.Verify(repo => repo.Save(It.IsAny<OrderModel>()), Times.Once);
        }

        [TestMethod]
        public void DeleteOrderLine_ReturnsSuccess_AfterOrderLineDeleted()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            // Act
            var result = _target.DeleteOrderLine(orderId, orderLineId);

            // Assert
            Assert.IsTrue(result.IsSuccessful);
        }

        [TestMethod]
        public void DeleteOrderLine_ReturnsInternalServerError_IfExceptionThrown()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            _mockRepository.Setup(repo => repo.Save(It.IsAny<OrderModel>())).Throws(new Exception());

            // Act
            var result = _target.DeleteOrderLine(orderId, orderLineId);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual(ServiceError.InternalServerError, result.ServiceError);
            Assert.IsTrue(result.ErrorMessages.Contains(deletingOrderLineError));
        }

        [TestMethod]
        public void DeleteOrderLine_LogsError_IfExceptionThrown()
        {
            // Arrange
            var repoOrder = new OrderModel { Id = orderId, Lines = new List<OrderLineModel> { GetOrderLine(orderLineId) } };
            _mockRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<OrderModel, bool>>>())).Returns(new List<OrderModel> { repoOrder });

            _mockRepository.Setup(repo => repo.Save(It.IsAny<OrderModel>())).Throws(new Exception(exceptionMessage));

            string loggedMessage = null;
            _mockLogger.Setup(logger => logger.LogError(It.IsAny<string>())).Callback((string str) => loggedMessage = str);

            // Act
            _target.DeleteOrderLine(orderId, orderLineId);

            // Assert
            Assert.IsNotNull(loggedMessage);
            Assert.IsTrue(loggedMessage.StartsWith($"{nameof(OrderService)}.{nameof(OrderService.DeleteOrderLine)}"));
            Assert.IsTrue(loggedMessage.Contains(deletingOrderLineError));
            Assert.IsTrue(loggedMessage.Contains(exceptionMessage));
        }

        #endregion DeleteOrderLine
    }
}
