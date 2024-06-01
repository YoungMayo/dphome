using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;
using CustomerMicroservice.Models;

namespace CustomerMicroservice.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("[controller]/[action]")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;
        private readonly PublisherServiceApiClient _publisherService;

        public AdminController(ILogger<AdminController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _publisherService = PublisherServiceApiClient.Create();
        }

        [HttpPost]
        public async Task<IActionResult> AddUpcomingVideo([FromBody] UpcomingVideoModel upcomingVideo)
        {
            var topicName = TopicName.FromProjectTopic(_configuration["GoogleCloud:ProjectId"], _configuration["GoogleCloud:PubSub:UpcomingVideosTopic"]);

            string messageJson = JsonSerializer.Serialize(upcomingVideo);
            var message = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(messageJson)
            };

            await _publisherService.PublishAsync(topicName, new[] { message });

            _logger.LogInformation($"Published upcoming video message: {messageJson}");

            return Ok(new { Status = "Success", Message = "Upcoming video added and notification sent." });
        }
    }
}
