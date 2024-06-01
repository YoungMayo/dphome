namespace VideoCatalogueMicroservice.Models
{
    public class Video
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; }
        public string ImageUrl { get; set; }
        public int ReleaseYear { get; set; }
        public string PosterUrl { get; set; }
    }
}
