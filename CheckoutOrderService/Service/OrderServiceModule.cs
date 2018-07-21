using CheckoutOrderService.Dependencies;
using Ninject.Modules;

namespace CheckoutOrderService.Service
{
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
