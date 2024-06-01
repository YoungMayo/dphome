using Newtonsoft.Json;
using System.Collections.Generic;

namespace VideoCatalogueMicroservice.Models

{
    public class TitleResult
    {
        public string Id { get; set; }
        public PrimaryImage PrimaryImage { get; set; }
        public TitleType TitleType { get; set; }
        public TitleText TitleText { get; set; }
        public ReleaseYear ReleaseYear { get; set; }
    }


    public class PrimaryImage
    {
        public string Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Url { get; set; }
        public Caption Caption { get; set; }
    }

    public class Caption
    {
        public string PlainText { get; set; }  
    }

    public class TitleText
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class ReleaseYear
    {
        [JsonProperty("year")]
        public int Year { get; set; }
    }

    public class TitleType
    {
        public string Text { get; set; }
        public string Id { get; set; }
        public bool IsSeries { get; set; }
        public bool IsEpisode { get; set; }
    }


    public class GenreResponse
    {
        public List<TitleResult> Results { get; set; }
    }

    public class SingleResultGenreResponse
    {
        public TitleResult Results { get; set; }
    }

    public class VideoSearchResult
    {
        [JsonProperty("results")]
        public List<Video> Results { get; set; }
    }

    public class MovieSearchResult
    {
        [JsonProperty("results")]
        public List<Movie> Results { get; set; }
    }


    public class Movie
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("titleText")]
        public TitleText TitleText { get; set; }

        [JsonProperty("releaseYear")]
        public ReleaseYear ReleaseYear { get; set; }
    }

}
