using CheckoutOrderService.Models;
using FluentValidation;
using System.Collections.Generic;
using System.Linq;

namespace CheckoutApi.Validation
{
    public class OrderValidator : AbstractValidator<Order>
    {
        /// <summary>
        /// Basic validation of <see cref="Order"/> objects
        /// </summary>
        public OrderValidator()
        {
            When(order => order.Lines != null, () =>
                RuleFor(order => order.Lines)
                    .SetCollectionValidator(new OrderLineValidator())
                    .Must(NotHaveDuplicateSkuCodes)
                    .WithMessage($"Duplicate {nameof(OrderLine.SkuCode)} in order"));
        }

        /// <summary>
        /// Verifies uniqueness of the <see cref="OrderLine.SkuCode"/>s in the order
        /// </summary>
        public static bool NotHaveDuplicateSkuCodes(IEnumerable<OrderLine> values)
        {
            return values.Count() == values.Select(line => line.SkuCode).Distinct().Count();
        }
    }

    public class OrderLineValidator : AbstractValidator<OrderLine>
    {
        /// <summary>
        /// Basic validation of <see cref="OrderLine"/> objects
        /// </summary>
        public OrderLineValidator()
        {
            RuleFor(line => line.SkuCode)
                .Must(NotBeNullOrWhiteSpace)
                .WithMessage($"{nameof(OrderLine.SkuCode)} is required");

            RuleFor(line => line.Quantity)
                .GreaterThan(0)
                .WithMessage($"Positive {nameof(OrderLine.Quantity)} is required");
        }

        /// <summary>
        /// Saves duplicate call to WithMessage() with each usage
        /// Saves call to Cascade() if the content of the string isn't also being validated
        /// </summary>
        private static bool NotBeNullOrWhiteSpace(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}
