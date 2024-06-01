using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using OrderMicroservice.Models;
using OrderMicroservice.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderMicroservice.Services
{
    public class WatchlistService : IWatchlistService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<WatchlistService> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly CollectionReference _watchlistCollection;

        public WatchlistService(FirestoreDb firestoreDb, ILogger<WatchlistService> logger, IOrderRepository orderRepository)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
            _orderRepository = orderRepository;
            _watchlistCollection = _firestoreDb.Collection("watchlist");
        }

        public async Task AddToWatchlistAsync(WatchlistItem watchlistItem)
        {
            try
            {
                DocumentReference docRef = _watchlistCollection.Document();
                await docRef.SetAsync(watchlistItem);
                _logger.LogInformation($"Added to watchlist: {watchlistItem.MovieName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding to watchlist: {ex.Message}");
                throw;
            }
        }

        public async Task<List<WatchlistItem>> GetWatchlistByUserEmailAsync(string userEmail)
        {
            var watchlistItems = new List<WatchlistItem>();
            var query = _watchlistCollection.WhereEqualTo("UserEmail", userEmail);
            var querySnapshot = await query.GetSnapshotAsync();
            foreach (var document in querySnapshot.Documents)
            {
                var item = document.ConvertTo<WatchlistItem>();
                watchlistItems.Add(item);
            }
            return watchlistItems;
        }

        public async Task RemoveFromWatchlistAsync(string userEmail, string movieName)
        {
            try
            {
                var query = _watchlistCollection.WhereEqualTo("UserEmail", userEmail).WhereEqualTo("MovieName", movieName);
                var querySnapshot = await query.GetSnapshotAsync();
                foreach (var document in querySnapshot.Documents)
                {
                    await document.Reference.DeleteAsync();
                    _logger.LogInformation($"Removed from watchlist: {document.Id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing from watchlist: {ex.Message}");
                throw;
            }
        }

        public async Task CleanUpWatchlistAsync(string userEmail)
        {
            _logger.LogInformation($"Starting CleanUpWatchlistAsync for userEmail: {userEmail}");
            var watchlistItems = await GetWatchlistByUserEmailAsync(userEmail);
            var orders = await _orderRepository.GetOrdersByUserEmailAsync(userEmail);

            _logger.LogInformation($"Found {watchlistItems.Count} watchlist items and {orders.Count} orders for userEmail: {userEmail}");

            foreach (var item in watchlistItems)
            {
                _logger.LogInformation($"Checking watchlist item: {item.MovieName}");
                var matchingOrder = orders.FirstOrDefault(order => order.MovieName == item.MovieName && order.IsPaid);
                if (matchingOrder != null)
                {
                    _logger.LogInformation($"Removing watchlist item: {item.MovieName} for user: {userEmail}");
                    await RemoveFromWatchlistAsync(userEmail, item.MovieName);
                }
                else
                {
                    _logger.LogInformation($"No matching paid order found for watchlist item: {item.MovieName} for user: {userEmail}");
                }
            }
            _logger.LogInformation($"Completed CleanUpWatchlistAsync for userEmail: {userEmail}");
        }
    }
}
