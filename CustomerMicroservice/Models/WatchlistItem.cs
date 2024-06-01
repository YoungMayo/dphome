using Google.Cloud.Firestore;

namespace OrderMicroservice.Models
{
    [FirestoreData]
    public class WatchlistItem
    {
        [FirestoreProperty]
        public string UserEmail { get; set; }

        [FirestoreProperty]
        public string MovieName { get; set; }
    }
}
