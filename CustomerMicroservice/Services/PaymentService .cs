using Google.Cloud.Firestore;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using OrderMicroservice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderMicroservice.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly PublisherServiceApiClient _publisher;
        private readonly string _projectId = "dphome-424621"; // project ID

        public PaymentService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
            _publisher = PublisherServiceApiClient.Create();
        }

        public async Task<List<Order>> GetUnpaidOrdersAsync()
        {
            var ordersRef = _firestoreDb.Collection("orders");
            var query = ordersRef.WhereEqualTo("IsPaid", false);
            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents.Select(doc => doc.ConvertTo<Order>()).ToList();
        }

        public async Task MarkOrderAsPaidAsync(string orderId)
        {
            var orderRef = _firestoreDb.Collection("orders").Document(orderId);
            var updates = new Dictionary<string, object>
            {
                { "IsPaid", true },
                { "DateTimeOfPayment", DateTime.UtcNow }
            };

            await orderRef.UpdateAsync(updates);

            // Publish the event to Pub/Sub
            var order = await orderRef.GetSnapshotAsync();
            var orderData = order.ConvertTo<Order>();
            var topicName = TopicName.FromProjectTopic(_projectId, "order-confirmation");
            var message = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(JsonSerializer.Serialize(orderData))
            };
            await _publisher.PublishAsync(topicName, new[] { message });
        }

        public async Task<List<Order>> GetPaidOrdersAsync()
        {
            var ordersRef = _firestoreDb.Collection("orders");
            var query = ordersRef.WhereEqualTo("IsPaid", true);
            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents.Select(doc => doc.ConvertTo<Order>()).ToList();
        }
    }
}
