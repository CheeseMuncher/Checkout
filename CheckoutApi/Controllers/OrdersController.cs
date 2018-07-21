using AutoMapper;
using CheckoutOrderService;
using CheckoutOrderService.Common;
using CheckoutOrderService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;

namespace CheckoutApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public OrdersController(IOrderService orderService, IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
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
            var response = _orderService.GetOrder(orderId);
            if (!response.IsSuccessful)
            {
                if (response.ServiceError == ServiceError.NotFound)
                {
                    return NotFound(string.Join(", ", response.ErrorMessages));
                }
                if (response.ServiceError == ServiceError.InternalServerError)
                {
                    return InternalServerError();
                }
            }
            var order = _mapper.Map<OrderModel, Order>(response.Data);
            return Ok(order);
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

        private ObjectResult InternalServerError()
        {
            return new ObjectResult(null)
            {
                StatusCode = (short)HttpStatusCode.InternalServerError
            };
        }
    }
}
