using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using OrderMicroservice.Models;
using OrderMicroservice.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderMicroservice.Repositories
{
    public class WatchlistRepository : IWatchlistRepository
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<WatchlistRepository> _logger;
        private readonly CollectionReference _watchlistCollection;

        public WatchlistRepository(FirestoreDb firestoreDb, ILogger<WatchlistRepository> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
            _watchlistCollection = _firestoreDb.Collection("watchlist");
        }

        public async Task AddToWatchlistAsync(WatchlistItem watchlistItem)
        {
            DocumentReference docRef = _watchlistCollection.Document();
            await docRef.SetAsync(watchlistItem);
        }

        public async Task<List<WatchlistItem>> GetWatchlistByUserEmailAsync(string userEmail)
        {
            var querySnapshot = await _watchlistCollection.WhereEqualTo("UserEmail", userEmail).GetSnapshotAsync();
            var watchlistItems = new List<WatchlistItem>();
            foreach (var document in querySnapshot.Documents)
            {
                watchlistItems.Add(document.ConvertTo<WatchlistItem>());
            }
            return watchlistItems;
        }

        public async Task RemoveFromWatchlistAsync(string id)
        {
            DocumentReference docRef = _watchlistCollection.Document(id);
            await docRef.DeleteAsync();
        }
    }
}
