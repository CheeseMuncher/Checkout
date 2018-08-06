using CheckoutOrderService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace CheckoutOrderService.Dependencies
{
    /// <inheritdoc />
    public class Repository : IRepository
    {
        private readonly string path = $"{Environment.CurrentDirectory}\\Resources\\DemoData.json";

        /// <inheritdoc />
        public IEnumerable<T> Get<T>(Expression<Func<T, bool>> predicate)
        {
            if (typeof(T) == typeof(SkuModel))
            {
                return GetDemoSkus() as IEnumerable<T>;
            }
            if (typeof(T) == typeof(OrderModel))
            {
                return predicate == null
                    ? GetDemoOrders() as IEnumerable<T>
                    : GetDemoOrders(predicate.Compile() as Func<OrderModel, bool>) as IEnumerable<T>;
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

        /// <summary>
        /// Need hard coded data for demo
        /// TODO Remove when repository is wired up
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
        /// Need hard coded data for demo
        /// TODO Remove when repository is wired up
        /// </summary>  
        private OrderModel[] GetNewDemoOrders()
        {
            var order1 = new OrderModel
            {
                Id = 1,
                Lines = new List<OrderLineModel>
                    {
                        new OrderLineModel(1, new SkuModel("A1", "Product A1")) { SortOrder = 10, Quantity = 2 },
                        new OrderLineModel(2, new SkuModel("A2", "Product A2")) { SortOrder = 20, Quantity = 4 },
                        new OrderLineModel(3, new SkuModel("B1", "Product B1")) { SortOrder = 30, Quantity = 8 }
                    }
            };
            var order2 = new OrderModel
            {
                Id = 2,
                Lines = new List<OrderLineModel>
                    {
                        new OrderLineModel(4, new SkuModel("A3", "Product A3")) { SortOrder = 10, Quantity = 3 },
                        new OrderLineModel(5, new SkuModel("B1", "Product B1")) { SortOrder = 20, Quantity = 5 }
                    }
            };
            return new[] { order1, order2 };
        }

        /// <summary>
        /// A json file based implementation of IRepository
        /// </summary>
        /// <returns></returns>
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

        private OrderModel[] GetDemoOrders(Func<OrderModel, bool> func)
        {
            return GetDemoOrders().Where(func).ToArray();
        }
    }

}
