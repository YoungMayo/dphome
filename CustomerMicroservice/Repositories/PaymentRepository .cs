using Google.Cloud.Firestore;
using OrderMicroservice.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderMicroservice.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly FirestoreDb _firestoreDb;

        public PaymentRepository(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<List<Order>> GetUnpaidOrdersAsync()
        {
            var unpaidOrders = new List<Order>();
            var query = _firestoreDb.Collection("orders").WhereEqualTo("IsPaid", false);
            var snapshot = await query.GetSnapshotAsync();

            foreach (var document in snapshot.Documents)
            {
                unpaidOrders.Add(document.ConvertTo<Order>());
            }

            return unpaidOrders;
        }

        public async Task MarkOrderAsPaidAsync(string orderId, DateTime paymentDateTime)
        {
            var orderRef = _firestoreDb.Collection("orders").Document(orderId);
            var updates = new Dictionary<string, object>
            {
                { "IsPaid", true },
                { "DateTimeOfPayment", paymentDateTime }
            };

            await orderRef.UpdateAsync(updates);
        }
    }
}
