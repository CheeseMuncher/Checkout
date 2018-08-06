using AutoMapper;
using CheckoutApi.Models;
using CheckoutOrderService;
using CheckoutOrderService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;

namespace CheckoutApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkusController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public SkusController(IOrderService orderService, IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
        }

        [HttpGet(Name = "GetSkus")]
        public ActionResult<IEnumerable<SkuModel>> GetSkus()
        {
            var response = _orderService.GetSkus();
            if (!response.IsSuccessful)
            {
                return new ObjectResult(null)
                {
                    StatusCode = (short)HttpStatusCode.InternalServerError
                };
            }
            var skus = _mapper.Map<IEnumerable<SkuModel>, IEnumerable<Sku>>(response.Data);
            return Ok(skus);
        }
    }
}
