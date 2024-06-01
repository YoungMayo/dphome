using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerMicroservice.Models;
using Google.Cloud.Firestore;
using System.Text.Json;
using OrderMicroservice.Models;

public class UpcomingVideoSubscriber : BackgroundService
{
    private readonly ILogger<UpcomingVideoSubscriber> _logger;
    private readonly SubscriberServiceApiClient _subscriberService;
    private readonly FirestoreDb _firestoreDb;
    private readonly string _projectId;
    private readonly string _subscriptionId;

    public UpcomingVideoSubscriber(ILogger<UpcomingVideoSubscriber> logger, IConfiguration configuration, FirestoreDb firestoreDb)
    {
        _logger = logger;
        _subscriberService = SubscriberServiceApiClient.Create();
        _firestoreDb = firestoreDb;
        _projectId = configuration["GoogleCloud:ProjectId"];
        _subscriptionId = configuration["GoogleCloud:PubSub:UpcomingVideosSubscription"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(_projectId, _subscriptionId);
        _logger.LogInformation($"Starting to listen to Pub/Sub subscription: {subscriptionName}");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await _subscriberService.PullAsync(subscriptionName, returnImmediately: false, maxMessages: 10);
                foreach (var msg in response.ReceivedMessages)
                {
                    // Process the message
                    //_logger.LogInformation($"Received message {msg.Message.MessageId}: {msg.Message.Data.ToStringUtf8()}");
                    await HandleUpcomingVideoMessageAsync(msg.Message.Data.ToStringUtf8());

                    // Acknowledge the message
                    await _subscriberService.AcknowledgeAsync(subscriptionName, new[] { msg.AckId });
                }
            }
            catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.Cancelled)
            {
                //_logger.LogWarning("Subscriber cancelled");
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error in subscriber");
            }

            await Task.Delay(1000); // Wait a second before pulling messages again
        }
    }

    private async Task HandleUpcomingVideoMessageAsync(string message)
    {
        var upcomingVideo = JsonSerializer.Deserialize<UpcomingVideoModel>(message);
        _logger.LogInformation($"Handling upcoming video: {upcomingVideo.Title}, Genre: {upcomingVideo.Genre}");

        var watchlistQuery = _firestoreDb.Collection("watchlist").WhereEqualTo("Genre", upcomingVideo.Genre);
        var watchlistSnapshot = await watchlistQuery.GetSnapshotAsync();

        foreach (var document in watchlistSnapshot.Documents)
        {
            var watchlistItem = document.ConvertTo<WatchlistItem>();
            var notification = new Notification
            {
                UserId = watchlistItem.UserEmail,
                Message = $"A new {upcomingVideo.Genre} video is coming soon: {upcomingVideo.Title}",
                CreatedAt = DateTime.UtcNow
            };

            var notificationRef = _firestoreDb.Collection("notifications").Document();
            await notificationRef.SetAsync(notification);
        }

        var orderQuery = _firestoreDb.Collection("orders").WhereEqualTo("Genre", upcomingVideo.Genre);
        var orderSnapshot = await orderQuery.GetSnapshotAsync();

        foreach (var document in orderSnapshot.Documents)
        {
            var order = document.ConvertTo<Order>();
            var notification = new Notification
            {
                UserId = order.UserEmail,
                Message = $"A new {upcomingVideo.Genre} video is coming soon: {upcomingVideo.Title}",
                CreatedAt = DateTime.UtcNow
            };

            var notificationRef = _firestoreDb.Collection("notifications").Document();
            await notificationRef.SetAsync(notification);
        }
    }
}
