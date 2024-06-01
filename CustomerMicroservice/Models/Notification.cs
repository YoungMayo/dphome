using Google.Cloud.Firestore;
using System;

namespace CustomerMicroservice.Models
{
    [FirestoreData]
    public class Notification
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string Message { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty]
        public bool IsRead { get; set; } = false;
    }
}
