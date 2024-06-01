using OrderMicroservice.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderMicroservice.Services
{
    public interface IWatchlistRepository
    {
        Task AddToWatchlistAsync(WatchlistItem watchlistItem);
        Task<List<WatchlistItem>> GetWatchlistByUserEmailAsync(string userEmail);
        Task RemoveFromWatchlistAsync(string id);
    }
}
