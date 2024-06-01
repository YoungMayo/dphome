using OrderMicroservice.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderMicroservice.Services
{
    public interface IOrderRepository
    {
        Task CreateOrderAsync(Order order);
        Task<List<Order>> GetOrdersAsync();
        Task<Order> GetOrderByIdAsync(string id);
        Task UpdateOrderAsync(Order order);
        Task<List<Order>> GetOrdersByUserEmailAsync(string userEmail);
    }
}
