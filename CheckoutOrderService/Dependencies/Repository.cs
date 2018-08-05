using CheckoutOrderService.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CheckoutOrderService.Dependencies
{
    /// <inheritdoc />
    public class Repository : IRepository
    {
        /// <inheritdoc />
        public IEnumerable<T> Get<T>(Expression<Func<T, bool>> predicate)
        {
            if (typeof(T) == typeof(SkuModel))
            {
                return GetDemoSkus() as IEnumerable<T>;
            }
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public T Save<T>(T value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Delete<T>()
        {
            throw new NotImplementedException();
        }

        private SkuModel[] GetDemoSkus()
        {
            return new[]
            {
                new SkuModel("A1", "Product A1"),
                new SkuModel("A2", "Product A2"),
                new SkuModel("A3", "Product A3"),
                new SkuModel("B1", "Product B1"),
            };
        }
    }
}
