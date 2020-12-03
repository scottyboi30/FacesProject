using System;
using System.Threading.Tasks;
using Faces.OrderApi.Data;
using Faces.OrderApi.Hubs;
using Faces.OrderApi.Models.Entities;
using MassTransit;
using Messaging.InterfacesConstants.Events;
using Microsoft.AspNetCore.SignalR;

namespace Faces.OrderApi.Messages.Consumers
{
    public class OrderDispatchedEventConsumer : IConsumer<IOrderDispatchedEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHubContext<OrderHub> _hubContext;
        public OrderDispatchedEventConsumer(IOrderRepository orderRepository, IHubContext<OrderHub> hubContext)
        {
            _orderRepository = orderRepository;
            _hubContext = hubContext;
        }
         
        public async Task Consume(ConsumeContext<IOrderDispatchedEvent> context)
        {
            var message = context.Message;
            var orderId = message.OrderId;
            await UpdateDatabase(orderId);
            await _hubContext.Clients.All.SendAsync("UpdateOrders",  "Dispatched", orderId );
        }

        private async Task UpdateDatabase(Guid orderId)
        {
            var order = await _orderRepository.GetOrderAsync(orderId);
            if(order !=null)
            {
                order.Status = Status.Sent;
                await _orderRepository.UpdateOrder(order);
            }
        }
    }
}
