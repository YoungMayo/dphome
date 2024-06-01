using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoCatalogueMicroservice.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace VideoCatalogueMicroservice.Services
{
    public class VideoCatalogueService : IVideoCatalogueService
    {
        private readonly string _apiUrl = "https://moviesdatabase.p.rapidapi.com";
 private readonly string _apiKey = "0310f79b2emsh87445576d4d0b10p15e44bjsnc100448154f2";
        private readonly ILogger<VideoCatalogueService> _logger;
        private readonly IConfiguration _configuration;

        public VideoCatalogueService(IConfiguration configuration, ILogger<VideoCatalogueService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiKey = _configuration["RapidApi:Key"];
        }

        public async Task<List<Video>> GetVideosByGenreAsync(string genre)
        {
            genre = char.ToUpper(genre[0]) + genre.Substring(1).ToLower(); // Normalize the genre input

            var client = new RestClient(_apiUrl);
            var request = new RestRequest("titles", Method.Get);
            request.AddParameter("genre", genre);
            request.AddHeader("X-RapidAPI-Key", "0310f79b2emsh87445576d4d0b10p15e44bjsnc100448154f2");
            request.AddHeader("X-RapidAPI-Host", "moviesdatabase.p.rapidapi.com");

            _logger.LogInformation("Sending API request for genre: {Genre}", genre);
            _logger.LogInformation("Request URL: {Url}", client.BuildUri(request));
            foreach (var header in request.Parameters.Where(p => p.Type == ParameterType.HttpHeader))
            {
                _logger.LogInformation("Request Header: {Name} = {Value}", header.Name, header.Value);
            }

            var response = await client.ExecuteAsync(request);
            _logger.LogInformation("Raw API Response: {ResponseContent}", response.Content);

            if (string.IsNullOrEmpty(response.Content))
            {
                _logger.LogWarning("No content received from the API.");
                return new List<Video>();
            }

            // Proceed with deserialization and other logic
            GenreResponse genreResponse = null;

            try
            {
                genreResponse = JsonConvert.DeserializeObject<GenreResponse>(response.Content);
                _logger.LogInformation("Deserialized List Response: {@ResponseData}", genreResponse);
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "Failed to deserialize list response.");
                return new List<Video>();
            }

            List<TitleResult> results = genreResponse?.Results ?? new List<TitleResult>();

            if (results.Count == 0)
            {
                _logger.LogWarning("No results found for genre: {Genre}", genre);
            }

            var filteredVideos = results.Select(v => new Video
            {
                Id = v.Id,
                Title = v.TitleText?.Text,
                Description = v.TitleType?.Text,
                Genre = genre,
                ImageUrl = v.PrimaryImage?.Url,
                ReleaseYear = v.ReleaseYear?.Year ?? 0,
                PosterUrl = v.PrimaryImage?.Url
            }).ToList();

            _logger.LogInformation("Filtered Videos: {@FilteredVideos}", filteredVideos);

            return filteredVideos;
        }

        public async Task<Video> GetVideoDetailsAsync(string id)
        {
            using var client = new HttpClient();
            var apiKey = _configuration["RapidApi:Key"];
            var apiHost = _configuration["RapidApi:Host"];

            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", apiHost);

            var requestUrl = $"https://moviesdatabase.p.rapidapi.com/titles/{id}";
            _logger.LogInformation("Request URL: {RequestUrl}", requestUrl);

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(requestUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP request failed.");
                throw;
            }

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API Request failed with status code {StatusCode}: {ResponseContent}", response.StatusCode, responseContent);
                response.EnsureSuccessStatusCode();
            }

            var responseData = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Raw API Response: {ResponseData}", responseData);

            SingleResultGenreResponse apiResponse;
            try
            {
                apiResponse = JsonConvert.DeserializeObject<SingleResultGenreResponse>(responseData);
                _logger.LogInformation("Deserialized API Response: {@ApiResponse}", apiResponse);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization failed.");
                throw;
            }

            if (apiResponse == null || apiResponse.Results == null)
            {
                _logger.LogError("Deserialized API response is null or missing results.");
                throw new Exception("Deserialized API response is null or missing results.");
            }

            var titleResult = apiResponse.Results;

            var video = new Video
            {
                Id = titleResult.Id ?? "Unknown ID",
                Title = titleResult.TitleText?.Text ?? "Unknown Title",
                ReleaseYear = titleResult.ReleaseYear?.Year ?? 0,
                Description = titleResult.PrimaryImage?.Caption?.PlainText ?? "No Description",
                ImageUrl = titleResult.PrimaryImage?.Url ?? "No Image URL"
            };

            _logger.LogInformation("Mapped Video Object: {@Video}", video);
            return video;
        }

        public class ApiResponse
        {
            public TitleResult Results { get; set; }
        }

        public async Task<IEnumerable<Movie>> SearchMoviesAsync(string query)
        {
            var client = new RestClient(_apiUrl);
            var request = new RestRequest("titles/search/title/" + query, Method.Get);
            request.AddHeader("X-RapidAPI-Key", _apiKey);
            request.AddHeader("X-RapidAPI-Host", "moviesdatabase.p.rapidapi.com");

            _logger.LogInformation("Searching for movies with query: {Query}", query);

            var response = await client.ExecuteAsync(request);
            _logger.LogInformation("API Response Status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("Movie search response: {Content}", response.Content);

            if (!response.IsSuccessful)
            {
                _logger.LogError("Failed to fetch movies: {ErrorMessage}", response.ErrorMessage);
                return new List<Movie>();
            }

            var movieSearchResult = JsonConvert.DeserializeObject<MovieSearchResult>(response.Content);

            // Filter results based on the query to simulate partial match behavior
            var filteredResults = movieSearchResult?.Results
                .Where(m => m.TitleText.Text.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<Movie>();

            return filteredResults;
        }


        public class SearchResponse
        {
            public List<TitleResult> Results { get; set; }
        }





    }
}
