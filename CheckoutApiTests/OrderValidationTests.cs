using CheckoutApi.Validation;
using CheckoutOrderService.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace CheckoutApiTests
{
    [TestClass]
    public class OrderLineValidatorTests
    {
        private OrderLineValidator _lineValidator => new OrderLineValidator();
        private OrderValidator _orderValidator => new OrderValidator();

        private readonly string skuMessage = $"{nameof(OrderLine.SkuCode)} is required";
        private readonly string quantityMessage = $"Positive {nameof(OrderLine.Quantity)} is required";

        #region Line Validation

        [TestMethod]
        public void Validate_ReturnsValid_IfOrderLineValid()
        {
            // Arrange
            var input = GetValidOrderLine();

            // Act
            var result = _lineValidator.Validate(input);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ReturnsInvalid_IfSkuCodeIsNull()
        {
            // Arrange
            var input = GetValidOrderLine();
            input.SkuCode = null;

            // Act
            var result = _lineValidator.Validate(input);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors.Single().ErrorMessage == skuMessage);
        }

        [TestMethod]
        public void Validate_ReturnsInvalid_IfSkuCodeIsEmpty()
        {
            // Arrange
            var input = GetValidOrderLine();
            input.SkuCode = string.Empty;

            // Act
            var result = _lineValidator.Validate(input);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors.Single().ErrorMessage == skuMessage);
        }

        [TestMethod]
        public void Validate_ReturnsInvalid_IfQuantityIsZero()
        {
            // Arrange
            var input = GetValidOrderLine();
            input.Quantity = 0;

            // Act
            var result = _lineValidator.Validate(input);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors.Single().ErrorMessage == quantityMessage);
        }

        [TestMethod]
        public void Validate_ReturnsInvalid_IfQuantityIsNegative()
        {
            // Arrange
            var input = GetValidOrderLine();
            input.Quantity = -1;

            // Act
            var result = _lineValidator.Validate(input);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors.Single().ErrorMessage == quantityMessage);
        }

        [TestMethod]
        public void Validate_ReturnsMultipleErrors_IfMultipleFailures()
        {
            // Arrange
            var input = GetValidOrderLine();
            input.Quantity = -1;
            input.SkuCode = null;

            // Act
            var result = _lineValidator.Validate(input);

            // Assert
            Assert.AreEqual(2, result.Errors.Count);
        }

        #endregion Line Validation

        [TestMethod]
        public void Validate_ReturnsValid_IfOrderValid()
        {
            // Arrange
            var input = GetValidOrder();

            // Act
            var result = _orderValidator.Validate(input);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ReturnsInvalid_IfSingleOrderLineInvalid()
        {
            // Arrange
            var input = GetValidOrder();
            input.Lines.First().Quantity = 0;

            // Act
            var result = _orderValidator.Validate(input);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors.Single().ErrorMessage == quantityMessage);
        }

        [TestMethod]
        public void Validate_ReturnsInvalid_IfMultipleOrderLinesInvalid()
        {
            // Arrange
            var input = GetValidOrder();
            input.Lines.First().Quantity = 0;
            input.Lines.Last().SkuCode = null;

            // Act
            var result = _orderValidator.Validate(input);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(2, result.Errors.Count);
            Assert.IsTrue(result.Errors.Any(error => error.ErrorMessage == quantityMessage));
            Assert.IsTrue(result.Errors.Any(error => error.ErrorMessage == skuMessage));
        }

        [TestMethod]
        public void Validate_ReturnsInvalid_IfOrderContainsDuplicatSkus()
        {
            // Arrange
            var input = GetValidOrder();
            input.Lines.Last().SkuCode = input.Lines.First().SkuCode;

            // Act
            var result = _orderValidator.Validate(input);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors.Single().ErrorMessage == $"Duplicate {nameof(OrderLine.SkuCode)} in order");
        }

        [TestMethod]
        public void Validate_ReturnsMultipleErrors_IfMultipleFailuresOnDifferentLevels()
        {
            // Arrange
            var input = GetValidOrder();
            input.Lines.Last().SkuCode = input.Lines.First().SkuCode;
            input.Lines.First().Quantity = 0;

            // Act
            var result = _orderValidator.Validate(input);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(2, result.Errors.Count);            
        }

        [TestMethod]
        public void Validate_HandlesNullLines()
        {
            // Arrange
            var input = GetValidOrder();
            input.Lines = null;

            // Act
            var result = _orderValidator.Validate(input);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        private OrderLine GetValidOrderLine(string code = "Code")
        {
            return new OrderLine
            {
                SkuCode = code,
                Quantity = 1
            };
        }

        private Order GetValidOrder()
        {
            return new Order
            {
                Lines = new List<OrderLine>
                {
                    GetValidOrderLine("Sku1"),
                    GetValidOrderLine("Sku2")
                }
            };
        }
    }
}
