using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VideoCatalogueMicroservice.Services;
using Microsoft.Extensions.Logging;
using VideoCatalogueMicroservice.Models;
using System.Net;

namespace VideoCatalogueMicroservice.Controllers
{
    [Route("[controller]/[action]")]
    public class VideoCatalogueController : Controller
    {
        private readonly IVideoCatalogueService _videoCatalogueService;
        private readonly ILogger<VideoCatalogueController> _logger;

        public VideoCatalogueController(IVideoCatalogueService videoCatalogueService, ILogger<VideoCatalogueController> logger)
        {
            _videoCatalogueService = videoCatalogueService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string genre)
        {
            if (string.IsNullOrEmpty(genre))
            {
                _logger.LogWarning("Genre input is empty.");
                return View(new List<Video>());
            }

            var videos = await _videoCatalogueService.GetVideosByGenreAsync(genre);
            _logger.LogInformation("Controller Received Videos: {@Videos}", videos);

            if (videos == null || videos.Count == 0)
            {
                _logger.LogWarning("No videos found for genre: {Genre}", genre);
            }

            return View(videos);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var video = await _videoCatalogueService.GetVideoDetailsAsync(id);
                if (video == null)
                {
                    _logger.LogWarning("Video not found for ID: {Id}", id);
                    return NotFound();
                }
                _logger.LogInformation("Video details: {@Video}", video);
                return View(video);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogError("Access to the API is forbidden. Please check  API key and permissions.");
                return StatusCode((int)HttpStatusCode.Forbidden, "Access to the API is forbidden. Please check  API key and permissions.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching video details.");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while fetching video details.");
            }
        }



        [HttpGet("search")]
        public async Task<IActionResult> SearchMovies([FromQuery] string query)
        {
            var movies = await _videoCatalogueService.SearchMoviesAsync(query);
            return Ok(movies);
        }



        [HttpGet("search")]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Query cannot be empty");
            }

            var movies = await _videoCatalogueService.SearchMoviesAsync(query);
            return Ok(movies);
        }



    }
}
