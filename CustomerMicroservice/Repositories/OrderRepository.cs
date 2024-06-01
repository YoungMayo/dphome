    using Google.Cloud.Firestore;
    using Microsoft.Extensions.Logging;
    using OrderMicroservice.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    namespace OrderMicroservice.Services
    {
        public class OrderRepository : IOrderRepository
        {
            private readonly FirestoreDb _firestoreDb;
            private readonly ILogger<OrderRepository> _logger;
            private readonly CollectionReference _ordersCollection;

            public OrderRepository(FirestoreDb firestoreDb, ILogger<OrderRepository> logger)
            {
                _firestoreDb = firestoreDb;
                _logger = logger;
                _ordersCollection = _firestoreDb.Collection("orders");
            }

            public async Task CreateOrderAsync(Order order)
            {
                try
                {
                    order.Id = Guid.NewGuid().ToString();
                    DocumentReference docRef = _firestoreDb.Collection("orders").Document(order.Id);
                    await docRef.SetAsync(order);
                    _logger.LogInformation($"Order with ID: {order.Id} created successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error creating order with ID: {order.Id}. Error: {ex.Message}");
                    throw;
                }
            }

            public async Task<List<Order>> GetOrdersAsync()
            {
                var orders = new List<Order>();
                var querySnapshot = await _firestoreDb.Collection("orders").GetSnapshotAsync();
                foreach (var document in querySnapshot.Documents)
                {
                    var order = document.ConvertTo<Order>();
                    orders.Add(order);
                }
                return orders;
            }

            public async Task<Order> GetOrderByIdAsync(string id)
            {
                var documentSnapshot = await _firestoreDb.Collection("orders").Document(id).GetSnapshotAsync();
                if (documentSnapshot.Exists)
                {
                    return documentSnapshot.ConvertTo<Order>();
                }
                return null;
            }

            public async Task UpdateOrderAsync(Order order)
            {
                try
                {
                    DocumentReference docRef = _firestoreDb.Collection("orders").Document(order.Id);
                    await docRef.SetAsync(order, SetOptions.Overwrite);
                    _logger.LogInformation($"Order with ID: {order.Id} updated successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating order with ID: {order.Id}. Error: {ex.Message}");
                    throw;
                }
            }

            public async Task<List<Order>> GetOrdersByUserEmailAsync(string userEmail)
            {
                var orders = new List<Order>();
                var query = _ordersCollection.WhereEqualTo("UserEmail", userEmail);
                var querySnapshot = await query.GetSnapshotAsync();
                foreach (var document in querySnapshot.Documents)
                {
                    var order = document.ConvertTo<Order>();
                    orders.Add(order);
                }
                return orders;
            }

        }
    }
