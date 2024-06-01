using Microsoft.AspNetCore.Mvc;
using OrderMicroservice.Services;
using System.Threading.Tasks;

namespace OrderMicroservice.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Index()
        {
            var unpaidOrders = await _paymentService.GetUnpaidOrdersAsync();
            return View(unpaidOrders);
        }

        [HttpPost]
        public async Task<IActionResult> Pay(string orderId)
        {
            await _paymentService.MarkOrderAsPaidAsync(orderId);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PaidOrders()
        {
            var paidOrders = await _paymentService.GetPaidOrdersAsync();
            return View(paidOrders);
        }


    }
}
