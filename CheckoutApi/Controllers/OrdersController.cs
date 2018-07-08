using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CheckoutApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "Hello", "World" };
        }
    }
}
