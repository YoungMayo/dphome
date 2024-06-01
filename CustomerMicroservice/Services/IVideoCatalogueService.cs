using System.Collections.Generic;
using System.Threading.Tasks;
using VideoCatalogueMicroservice.Models;

namespace VideoCatalogueMicroservice.Services
{
    public interface IVideoCatalogueService
    {
        Task<List<Video>> GetVideosByGenreAsync(string genre);
        Task<Video> GetVideoDetailsAsync(string id);
        Task<IEnumerable<Movie>> SearchMoviesAsync(string query);

    }
}
