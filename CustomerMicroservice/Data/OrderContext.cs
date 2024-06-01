using Microsoft.EntityFrameworkCore;
using OrderMicroservice.Models;

namespace OrderMicroservice.Data
{
    public class OrderContext : DbContext
    {
        public OrderContext(DbContextOptions<OrderContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        //public DbSet<OrderItem> OrderItems { get; set; }
    }
}
