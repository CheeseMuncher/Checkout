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
    }
}
