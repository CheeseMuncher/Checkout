using CheckoutOrderService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace CheckoutOrderService.Dependencies
{
    /// <inheritdoc />
    public class Repository : IRepository
    {
        /// <inheritdoc />
        public IEnumerable<T> Get<T>(Expression<Func<T, bool>> predicate)
        {
            return DemoGet(predicate);
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public T Save<T>(T value) where T : class
        {
            return DemoSave(value);
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Delete<T>()
        {
            throw new NotImplementedException();
        }

        #region Demo Implementation // TODO Remove the Demo implementation when the real repository is wired up. 

        /// <summary>
        /// Hard coded path for Demo implementation of <see cref="Repository"/>
        /// </summary>
        private readonly string path = $"{Environment.CurrentDirectory}\\Resources\\DemoData.json";

        /// <summary>
        /// Demo implementation of the <see cref="Repository"/> <see cref="IRepository.Get"/> method
        /// </summary>
        private IEnumerable<T> DemoGet<T>(Expression<Func<T, bool>> predicate)
        {
            if (typeof(T) == typeof(SkuModel))
            {
                return predicate == null
                    ? GetDemoSkus() as IEnumerable<T>
                    : GetDemoSkus(predicate.Compile() as Func<SkuModel, bool>) as IEnumerable<T>;
            }
            if (typeof(T) == typeof(OrderModel))
            {
                return predicate == null
                    ? GetDemoOrders() as IEnumerable<T>
                    : GetDemoOrders(predicate.Compile() as Func<OrderModel, bool>) as IEnumerable<T>;
            }
            return new T[0];
        }

        /// <summary>
        /// Demo implementation of the <see cref="Repository"/> <see cref="IRepository.Save"/> method
        /// </summary>
        private T DemoSave<T>(T value) where T : class
        {
            if (typeof(T) == typeof(OrderModel))
            {
                var order = value as OrderModel;
                var orders = GetDemoOrders();
                orders = UpdateOrderLineIds(orders, order);
                if (orders.All(o => o.Id != order.Id))
                {
                    var maxOrderId = orders.Select(o => o.Id).Max();
                    order = new OrderModel(++maxOrderId) { Lines = order.Lines };
                    orders = orders.Concat(new[] { order }).ToArray();
                }
                // Assumption: directory and file already exist
                File.WriteAllText(path, JsonConvert.SerializeObject(orders));
                return order as T;
            }
            return default(T);
        }

        /// <summary>
        /// Hard coded <see cref="SkuModel"/> data for demo
        /// </summary>        
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

        /// <summary>
        /// Demo implementation of the <see cref="Repository"/> <see cref="IRepository.Get"/> method for <see cref="SkuModel"/>s
        /// </summary>
        private SkuModel[] GetDemoSkus(Func<SkuModel, bool> func)
        {
            return GetDemoSkus().Where(func).ToArray();
        }

        /// <summary>
        /// Hard coded <see cref="OrderModel"/> data for demo
        /// This will break a test which should be removed along with an update to <see cref="SkuModel"/>
        /// </summary>  
        private OrderModel[] GetNewDemoOrders()
        {
            var order1 = new OrderModel(1)
            {
                Lines = new List<OrderLineModel>
                    {
                        new OrderLineModel(1, new SkuModel("A1", "Product A1")) { SortOrder = 10, Quantity = 2 },
                        new OrderLineModel(2, new SkuModel("A2", "Product A2")) { SortOrder = 20, Quantity = 4 },
                        new OrderLineModel(3, new SkuModel("B1", "Product B1")) { SortOrder = 30, Quantity = 8 }
                    }
            };
            var order2 = new OrderModel(2)
            {
                Lines = new List<OrderLineModel>
                    {
                        new OrderLineModel(4, new SkuModel("A3", "Product A3")) { SortOrder = 10, Quantity = 3 },
                        new OrderLineModel(5, new SkuModel("B1", "Product B1")) { SortOrder = 20, Quantity = 5 }
                    }
            };
            return new[] { order1, order2 };
        }

        /// <summary>
        /// Demo implementation of the <see cref="Repository"/> <see cref="IRepository.Get"/> method for <see cref="OrderModel"/>s
        /// </summary>
        private OrderModel[] GetDemoOrders()
        {
            if (!File.Exists(path))
            {
                if (!Directory.Exists(path.Substring(0, path.Length - 14)))
                {
                    Directory.CreateDirectory(path.Substring(0, path.Length - 14));
                }
                File.Create(path).Close();
                File.WriteAllText(path, JsonConvert.SerializeObject(GetNewDemoOrders()));
                return GetNewDemoOrders();
            }
            else
            {
                return JsonConvert.DeserializeObject<OrderModel[]>(File.ReadAllText(path));
            }
        }

        /// <summary>
        /// Demo implementation of the <see cref="Repository"/> <see cref="IRepository.Get"/> method for <see cref="OrderModel"/>s with predicate
        /// </summary>
        private OrderModel[] GetDemoOrders(Func<OrderModel, bool> func)
        {
            return GetDemoOrders().Where(func).ToArray();
        }

        /// <summary>
        /// Spoofs Identifier updates for the <see cref="OrderLineModel"/>s in the supplied <see cref="OrderModel"/> in a way that is consistent with an relational database
        /// </summary>
        private OrderModel[] UpdateOrderLineIds(OrderModel[] orders, OrderModel order)
        {
            var maxLineId = orders.SelectMany(o => o.Lines.Select(line => line.Id)).Max();
            var repoLines = order.Lines.Select(line =>
                line.Id != 0 ? line
                : new OrderLineModel(++maxLineId, GetDemoSkus().Where(sku => sku.Id == line.Sku.Id).FirstOrDefault())
                {
                    Quantity = line.Quantity,
                    SortOrder = line.SortOrder == 0 ? order.Lines.Select(l => l.SortOrder).Max() + 10 : line.SortOrder
                }).ToList();
            order.Lines = repoLines;
            return orders.Replace(order, o => o.Id == order.Id).ToArray();
        }
    }

    /// <summary>
    /// Provides extensions to System.Linq
    /// TODO move this to a utils or common project if we keep this after the repository is wired up
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Replaces all objects that match the supplied predicate with the supplied object
        /// </summary>
        public static IEnumerable<T> Replace<T>(this IEnumerable<T> sequence, T replace, Expression<Func<T, bool>> predicate)
        {
            var func = predicate.Compile();
            foreach (T item in sequence)
            {
                T value = func(item) ? replace : item;
                yield return value;
            }
        }
    }

    #endregion Demo Implementation
}
