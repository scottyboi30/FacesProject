using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Faces.OrderApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faces.OrderApi.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrdersContext _context;

        public OrderRepository(OrdersContext context)
        {
            _context = context;
        }

        public async Task<Order> GetOrderAsync(Guid id)
        {
            return await _context.Orders.
                Include("OrderDetails").
                FirstOrDefaultAsync(c => c.OrderId == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task RegisterOrder(Order order)
        {
            _context.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrder(Order order)
        {
            _context.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}
