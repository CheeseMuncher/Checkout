using AutoMapper;
using CheckoutOrderService;
using CheckoutOrderService.Common;
using CheckoutOrderService.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
//using System.Web.Http.Cors;


namespace CheckoutApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[EnableCors(origins: "http://localhost:45000/orders.html", headers: "*", methods: "*")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly IValidator<Order> _orderValidator;
        private readonly IValidator<OrderLine> _orderLineValidator;

        public OrdersController(IOrderService orderService, IMapper mapper, IValidator<Order> orderValidator, IValidator<OrderLine> orderLineValidator)
        {
            _orderService = orderService;
            _mapper = mapper;
            _orderValidator = orderValidator;
            _orderLineValidator = orderLineValidator;
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
                return ProcessServiceFailure(response) as ActionResult;
            }
            var order = _mapper.Map<OrderModel, Order>(response.Data);
            return Ok(order);
        }

        [HttpPost("{orderId}", Name = "CreateOrder")]
        public IActionResult Create(Order order)
        {
            var result = _orderValidator.Validate(order);
            if(!result.IsValid)
            {
                return BadRequest(string.Join(", ", result.Errors));
            }
            var model = _mapper.Map<Order, OrderModel>(order);
            var response = _orderService.CreateNewOrder(model);
            if (!response.IsSuccessful)
            {
                return ProcessServiceFailure(response);
            }
            return Created($"GetOrder/{response.Data}", $"{response.Data}");
        }

        [HttpDelete("{orderId}", Name = "ClearOrder")]
        public IActionResult Clear(int orderId, [FromQuery]int? orderLineId)
        {
            var response = orderLineId.HasValue
                ? _orderService.DeleteOrderLine(orderId, orderLineId.Value)
                : _orderService.ClearOrder(orderId);

            if (!response.IsSuccessful)
            {
                return ProcessServiceFailure(response) as ActionResult;
            }
            return Ok();
        }

        [HttpPut("{orderId}", Name = "UpdateOrderLine")]
        public IActionResult Update(int orderId, OrderLine orderLine)
        {
            var result = _orderLineValidator.Validate(orderLine);
            if (!result.IsValid)
            {
                return BadRequest(string.Join(", ", result.Errors));
            }
            var model = _mapper.Map<OrderLine, OrderLineModel>(orderLine);
            var response = _orderService.UpdateOrderLine(orderId, model);
            if(!response.IsSuccessful)
            {
                return ProcessServiceFailure(response);
            }
            // TODO update location if we create an Order Getter
            return Created(string.Empty, $"{response.Data}");
        }

        /// <summary>
        /// Reusable service failure handling
        /// TODO: move this to an abstract CheckoutApiControllerBase when we add another controller 
        /// </summary>
        private IActionResult ProcessServiceFailure(ServiceResponse response)
        {
            switch(response.ServiceError)
            {
                case ServiceError.NotFound:
                    return NotFound(string.Join(", ", response.ErrorMessages));

                case ServiceError.BadRequest:
                    return BadRequest(string.Join(", ", response.ErrorMessages));

                case ServiceError.InternalServerError:
                default:
                    return InternalServerError();
            }
        }

        /// <summary>
        /// InternalServerError initialise consistent with the other initialisers on <see cref="ContollerBase"/>
        /// TODO: move this to an abstract CheckoutApiControllerBase when we add another controller 
        /// </summary>
        private ObjectResult InternalServerError()
        {
            return new ObjectResult(null)
            {
                StatusCode = (short)HttpStatusCode.InternalServerError
            };
        }
    }
}
