using AutoMapper;
using CheckoutOrderService.Models;

namespace CheckoutApi.Mappers
{
    public class OrderMapper : Profile
    {
        public OrderMapper()
        {
            // Outbound Mappings
            CreateMap<OrderLineModel, OrderLine>()
                .ForMember(line => line.SkuCode, model => model.MapFrom(line => line.Sku.Id))
                .ForMember(line => line.SkuDisplayName, model => model.MapFrom(line => line.Sku.DisplayName));

            CreateMap<OrderModel, Order>();
        }
    }
}
