using AutoMapper;
using CheckoutApi.Models;
using CheckoutOrderService;
using CheckoutOrderService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;

namespace CheckoutApi.Controllers
{
    /// <summary>
    /// Operations relating to Skus
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
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

        /// <summary>
        /// Provides a collection of Skus definitions used to build orders 
        /// </summary>
        [HttpGet(Name = "GetSkus")]
        [ProducesResponseType(typeof(IEnumerable<Sku>), 200)]
        [ProducesResponseType(500)]
        public ActionResult<IEnumerable<Sku>> GetSkus()
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
