using CheckoutOrderService;
using CheckoutOrderService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CheckoutApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService orderService;

        public OrdersController(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        [Route("api/[controller]/{orderId:int}")]
        [HttpGet]
        public ActionResult<Order> Get(int orderId)
        {
            return null;
        }
    }
}
