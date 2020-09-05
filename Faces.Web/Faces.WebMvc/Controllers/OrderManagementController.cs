using System;
using System.Threading.Tasks;
using Faces.WebMvc.RestClients;
using Microsoft.AspNetCore.Mvc;

namespace Faces.WebMvc.Controllers
{
    [Route("OrderManagement")]
    public class OrderManagementController : Controller
    {
        private readonly IOrderApi _ordersApi;

        public OrderManagementController(IOrderApi ordersApi)
        {
            _ordersApi = ordersApi;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _ordersApi.GetOrders();
            foreach (var order in orders)
            {
                order.ImageString = ConvertAndFormatToString(order.ImageData);
            }

            return View(orders);
        }

        [HttpGet]
        [Route("/Details/{orderId}")]
        public async Task<IActionResult> Details(string orderId)
        {
            var order = await _ordersApi.GetOrderById(Guid.Parse(orderId));
            order.ImageString = ConvertAndFormatToString(order.ImageData);

            foreach (var detail in order.OrderDetails)
            {
                detail.ImageString = ConvertAndFormatToString(detail.FaceData);
            }

            return View(order);
        }

        private static string ConvertAndFormatToString(byte[] imageData)
        {
            var imageBase64Data = Convert.ToBase64String(imageData);
            return $"data:image/png;base64, {imageBase64Data}";
        }
    }
}
