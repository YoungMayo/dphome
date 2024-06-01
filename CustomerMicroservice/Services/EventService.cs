/*using OrderMicroservice.Models;
using OrderMicroservice.Data;

namespace OrderMicroservice.Services
{
    public class EventService : IEventService
    {
        private readonly OrderContext _context;

        public EventService(OrderContext context)
        {
            _context = context;
        }

        public void HandleOrderCreatedEvent(Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();

            Console.WriteLine($"Payment logged for order {order.Id}");

            Console.WriteLine($"Customer notified for order {order.Id}");
        }
    }
}*/