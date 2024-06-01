using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderMicroservice.Models;

namespace OrderMicroservice.Repositories
{
    public interface IPaymentRepository
    {
        Task<List<Order>> GetUnpaidOrdersAsync();
        Task MarkOrderAsPaidAsync(string orderId, DateTime paymentDateTime);
    }
}
