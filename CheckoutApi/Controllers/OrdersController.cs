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
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Order>> Get()
        {
            return new List<Order>
            {
                new Order { Lines = new List<OrderLine> { new OrderLine { SkuCode = "Hello World 1" } } },
                new Order { Lines = new List<OrderLine> { new OrderLine { SkuCode = "Hello World 2" } } },
                new Order { Lines = new List<OrderLine> { new OrderLine { SkuCode = "Hello World 3" } } }
            };                
        }

        [HttpGet("{orderId}", Name = "GetOrder")]
        public ActionResult<Order> Get(int orderId)
        {
            return new Order { Lines = new List<OrderLine> { new OrderLine { SkuCode = $"Hello World {orderId}" } } };
        }

        [HttpPost("{orderId}", Name = "CreateOrder")]
        public IActionResult Create(Order order)
        {
            return Created("GetOrder/7", "7");
        }

        [HttpDelete("{orderId}", Name = "ClearOrder")]
        public IActionResult Clear(int orderId, [FromQuery]int? orderLineId)
        {
            return Ok();
        }

        [HttpPut("{orderId}", Name = "UpdateOrderLine")]
        public IActionResult Update(int orderId, OrderLine orderLine)
        {
            return Ok();
        }
    }
}
