using System.Threading.Tasks;
using CustomerMicroservice.Models;

public interface IUserRepository
{
    Task CreateUserAsync(User user);
    Task<User> GetUserByEmailAsync(string email);
    Task<User> GetUserByIdAsync(string userId);
    Task AddNotificationAsync(Notification notification);
    Task<List<Notification>> GetNotificationsAsync(string userId);
    Task MarkNotificationAsReadAsync(string notificationId);
}
