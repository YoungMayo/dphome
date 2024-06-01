using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerMicroservice.Models;

public class UserRepository : IUserRepository
{
    private readonly FirestoreDb _firestoreDb;
    private const string UsersCollectionName = "users";
    private const string NotificationsCollectionName = "notifications"; // Add this line

    public UserRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task CreateUserAsync(User user)
    {
        DocumentReference docRef = _firestoreDb.Collection(UsersCollectionName).Document(user.Id);
        await docRef.SetAsync(user);
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        Query query = _firestoreDb.Collection(UsersCollectionName).WhereEqualTo("Email", email);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<User>()).FirstOrDefault();
    }

    public async Task<User> GetUserByIdAsync(string userId)
    {
        DocumentReference docRef = _firestoreDb.Collection(UsersCollectionName).Document(userId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<User>() : null;
    }

    public async Task AddNotificationAsync(Notification notification)
    {
        DocumentReference docRef = _firestoreDb.Collection(NotificationsCollectionName).Document();
        await docRef.SetAsync(notification);
    }

    public async Task<List<Notification>> GetNotificationsAsync(string userId)
    {
        Query query = _firestoreDb.Collection(NotificationsCollectionName).WhereEqualTo("UserId", userId);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Notification>()).ToList();
    }

    public async Task MarkNotificationAsReadAsync(string notificationId)
    {
        DocumentReference docRef = _firestoreDb.Collection(NotificationsCollectionName).Document(notificationId);
        await docRef.UpdateAsync(new Dictionary<string, object> { { "IsRead", true } });
    }
}
