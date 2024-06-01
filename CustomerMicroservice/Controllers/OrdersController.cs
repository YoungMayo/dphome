using Microsoft.AspNetCore.Mvc;
using OrderMicroservice.Models;
using OrderMicroservice.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;
using VideoCatalogueMicroservice.Services;

namespace OrderMicroservice.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IVideoCatalogueService _videoCatalogueService;
        private readonly IWatchlistService _watchlistService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderRepository orderRepository, IVideoCatalogueService videoCatalogueService, IWatchlistService watchlistService, ILogger<OrdersController> logger)
        {
            _orderRepository = orderRepository;
            _videoCatalogueService = videoCatalogueService;
            _watchlistService = watchlistService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            ModelState.Remove("Id");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for order creation. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(order);
            }

            var movies = await _videoCatalogueService.SearchMoviesAsync(order.MovieName);
            var movieExists = movies.Any(m => m.TitleText.Text == order.MovieName);

            if (!movieExists)
            {
                ModelState.AddModelError("MovieName", "Please select a valid movie name from the suggestions.");
                return View(order);
            }

            order.Id = Guid.NewGuid().ToString();
            order.OrderDate = DateTime.UtcNow;
            order.Cost = new Random().Next(1, 1000) / 100.0f;
            order.TotalPrice = order.Cost * order.Copies;
            order.IsPaid = false;
            order.DateTimeOfPayment = null; // Ensure this is set to null on creation

            await _orderRepository.CreateOrderAsync(order);
            _logger.LogInformation("Order created with ID {OrderId}", order.Id);

            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderRepository.GetOrdersAsync();
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Order ID cannot be null or empty.");
            }

            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Pay(string id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.IsPaid = true;
            order.DateTimeOfPayment = DateTime.UtcNow;

            await _orderRepository.UpdateOrderAsync(order);
            await _watchlistService.CleanUpWatchlistAsync(order.UserEmail);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PayOrder(string orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            order.IsPaid = true;
            order.DateTimeOfPayment = DateTime.UtcNow;

            await _orderRepository.UpdateOrderAsync(order);

            // Ensure the CleanUpWatchlistAsync method is called after payment
            await _watchlistService.CleanUpWatchlistAsync(order.UserEmail);

            return RedirectToAction("Index");
        }
    }
}
