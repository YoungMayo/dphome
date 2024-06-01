using Google.Cloud.Firestore;
using System;

namespace OrderMicroservice.Models
{
    [FirestoreData]
    public class Order
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string UserEmail { get; set; }

        [FirestoreProperty]
        public string MovieName { get; set; }

        [FirestoreProperty]
        public float Cost { get; set; }

        [FirestoreProperty]
        public int Copies { get; set; }

        [FirestoreProperty]
        public float TotalPrice { get; set; }

        [FirestoreProperty]
        public bool IsPaid { get; set; } = false; // Default to false

        [FirestoreProperty]
        public DateTime OrderDate { get; set; } // Add OrderDate property

        private DateTime? _dateTimeOfPayment;

        [FirestoreProperty]
        public DateTime? DateTimeOfPayment
        {
            get => _dateTimeOfPayment;
            set => _dateTimeOfPayment = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : (DateTime?)null;
        }
    }

    public class OrderMessage
    {
        public string OrderId { get; set; }
        public string UserEmail { get; set; }
        public bool IsPaid { get; set; }
        public string MovieName { get; set; }
    }
}