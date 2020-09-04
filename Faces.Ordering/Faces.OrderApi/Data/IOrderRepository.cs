using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Faces.OrderApi.Models.Entities;

namespace Faces.OrderApi.Data
{
    public interface IOrderRepository
    {
        Task<Order> GetOrderAsync(Guid id);
        Task<IEnumerable<Order>> GetOrdersAsync();
        Task RegisterOrder(Order order);
        Task UpdateOrder(Order order);
    }
}
