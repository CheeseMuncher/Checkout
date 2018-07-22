using AutoMapper;
using CheckoutApi.Mappers;
using CheckoutApi.Validation;
using CheckoutOrderService.Models;
using FluentValidation;
using Ninject;
using Ninject.Modules;

namespace CheckoutApi.IoC
{
    /// <summary>
    /// Defines all the bindings required by the API apart from service level dependencies which are defined at service level
    /// </summary>
    public class ApiModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IMapper>().ToMethod(AutoMapper).InSingletonScope();
            Bind<IValidator<Order>>().To<OrderValidator>();
        }
        
        private IMapper AutoMapper(Ninject.Activation.IContext context)
        {
            Mapper.Initialize(config =>
            {
                config.ConstructServicesUsing(type => context.Kernel.Get(type));
                config.AddProfile(new OrderMapper());
            });

            return Mapper.Instance;
        }
    }
}
