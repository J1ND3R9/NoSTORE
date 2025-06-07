using Microsoft.AspNetCore.Mvc;

namespace NoSTORE.Controllers
{
    [Route("Order")]
    public class OrderController : Controller
    {
        [HttpGet("Complete/{orderId}")]
        public IActionResult Complete(string orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }
    }
}
