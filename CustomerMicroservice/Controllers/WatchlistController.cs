using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using OrderMicroservice.Models;
using OrderMicroservice.Services;
using Microsoft.AspNetCore.Authorization;
using static Google.Cloud.Firestore.V1.StructuredQuery.Types;

namespace OrderMicroservice.Controllers
{
    [Authorize]
    public class WatchlistController : Controller
    {
        private readonly IWatchlistService _watchlistService;

        public WatchlistController(IWatchlistService watchlistService)
        {
            _watchlistService = watchlistService;
        }

        [HttpPost]
        public async Task<IActionResult> AddToWatchlist(string movieName)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name); // Changed to ClaimTypes.Name

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var watchlistItem = new WatchlistItem
            {
                //Id = Guid.NewGuid().ToString(), // Assign a new Guid to the Id property
                UserEmail = userEmail,
                MovieName = movieName
            };

            await _watchlistService.AddToWatchlistAsync(watchlistItem);

            return RedirectToAction("Index", "Watchlist");
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var watchlistItems = await _watchlistService.GetWatchlistByUserEmailAsync(userEmail);

            return View(watchlistItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanUpWatchlist()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            await _watchlistService.CleanUpWatchlistAsync(userEmail);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWatchlist(string movieName)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            await _watchlistService.RemoveFromWatchlistAsync(userEmail, movieName);
            return RedirectToAction("Index");
        }
    }
}
