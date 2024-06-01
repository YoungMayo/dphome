using OrderMicroservice.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderMicroservice.Services
{
    public interface IPaymentService
    {
        Task<List<Order>> GetUnpaidOrdersAsync();
        Task MarkOrderAsPaidAsync(string orderId);
        Task<List<Order>> GetPaidOrdersAsync(); // Ensure this line matches the implementation
    }
}
