using System;
using System.Threading.Tasks;
using Faces.OrderApi.Data;
using Faces.OrderApi.Models.Entities;
using MassTransit;
using Messaging.InterfacesConstants.Events;

namespace Faces.OrderApi.Messages.Consumers
{
    public class OrderDispatchedEventConsumer : IConsumer<IOrderDispatchedEvent>
    {
        private readonly IOrderRepository _orderRepository;
        public OrderDispatchedEventConsumer(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
         
        public async Task Consume(ConsumeContext<IOrderDispatchedEvent> context)
        {
            var message = context.Message;
            var orderId = message.OrderId;
            await UpdateDatabase(orderId);
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
