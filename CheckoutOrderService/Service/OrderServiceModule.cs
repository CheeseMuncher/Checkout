using CheckoutOrderService.Dependencies;
using Ninject.Modules;

namespace CheckoutOrderService.Service
{
    /// <summary>
    /// Defines all the bindings required for the OrderServiceModule
    /// </summary>
    public class OrderServiceModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IOrderService>().To<OrderService>();
            Bind<IRepository>().To<Repository>();
            Bind<ILogger>().To<Logger>();
        }
    }
}
