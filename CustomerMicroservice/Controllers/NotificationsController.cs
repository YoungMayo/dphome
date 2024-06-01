using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using CustomerMicroservice.Models;

namespace CustomerMicroservice.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class NotificationsController : Controller
    {
        private readonly IUserRepository _userRepository;

        public NotificationsController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var notifications = await _userRepository.GetNotificationsAsync(userId);
            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            await _userRepository.MarkNotificationAsReadAsync(notificationId);
            return RedirectToAction("Index");
        }
    }
}
