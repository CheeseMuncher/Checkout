using AutoMapper;
using CheckoutApi.Models;
using CheckoutOrderService.Models;

namespace CheckoutApi.Mappers
{
    public class SkuMapper : Profile
    {
        public SkuMapper()
        {
            // Outbound Mappings
            CreateMap<SkuModel, Sku>()
                .ForMember(sku => sku.Code, model => model.MapFrom(m => m.Id));
        }
    }
}
