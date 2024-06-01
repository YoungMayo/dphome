using OrderMicroservice.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderMicroservice.Services
{
    public interface IWatchlistService
    {
        Task AddToWatchlistAsync(WatchlistItem watchlistItem);
        Task<List<WatchlistItem>> GetWatchlistByUserEmailAsync(string userEmail);
        Task RemoveFromWatchlistAsync(string userEmail, string movieName);
        Task CleanUpWatchlistAsync(string userEmail);
    }
}

