using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CheckoutOrderService.Dependencies
{
    /// <summary>
    /// A generic persistence abstraction
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Fetches objects from our persistence layer using the supplied predicate
        /// </summary>
        IEnumerable<T> Get<T>(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Saves the supplied value to our persistence layer
        /// </summary>
        /// <returns>The saved object with updated identifier if new</returns>
        T Save<T>(T value);

        /// <summary>
        /// Deletes a single object from our persistence layer using the supplied identifier
        /// </summary>
        /// <typeparam name="T">The type to attempt to delete</typeparam>
        /// <returns>True for success, false otherwise</returns>
        bool Delete<T>();
    }
}
