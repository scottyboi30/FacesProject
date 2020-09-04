using Faces.OrderApi.Data;
using MassTransit;
using Messaging.InterfacesConstants.Commands;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Faces.OrderApi.Models.Entities;
using Newtonsoft.Json;

namespace Faces.OrderApi.Messages.Consumers
{
    public class RegisterOrderCommandConsumer : IConsumer<IRegisterOrderCommand>
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IHttpClientFactory _clientFactory;

        public RegisterOrderCommandConsumer(IOrderRepository orderRepo, IHttpClientFactory clientFactory)
        {
            _orderRepo = orderRepo;
            _clientFactory = clientFactory;

        }

        public async Task Consume(ConsumeContext<IRegisterOrderCommand> context)
        {
            var result = context.Message;
            if (result.OrderId != Guid.Empty && result.PictureUrl != null 
                                             && result.UserEmail != null && result.ImageData != null)
            {
                await SaveOrder(result);

                var client = _clientFactory.CreateClient();
                var (faces, orderId) = await GetFacesFromFaceApiAsync(client, result.ImageData, result.OrderId);
                await SaveOrderDetails(orderId, faces);
            }
        }

        private static async Task<Tuple<List<byte[]>, Guid>> GetFacesFromFaceApiAsync(HttpClient client, byte[] imageData, Guid orderId)
        {
            var byteContent = new ByteArrayContent(imageData);
            Tuple<List<byte[]>, Guid> orderDetailData = null;
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            using var response = await client.PostAsync("http://localhost:6001/api/faces?orderId=" + orderId, byteContent);
            var apiResponse = await response.Content.ReadAsStringAsync();
            orderDetailData = JsonConvert.DeserializeObject<Tuple<List<byte[]>, Guid>>(apiResponse);
            return orderDetailData;
        }

        private async Task SaveOrderDetails(Guid orderId, IEnumerable<byte[]> faces)
        {
            var order = _orderRepo.GetOrderAsync(orderId).Result;
            if (order != null)
            {
                order.Status = Status.Processed;
                foreach (var face in faces)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = orderId,
                        FaceData = face
                    };
                    order.OrderDetails.Add(orderDetail);
                }
            }

            await _orderRepo.UpdateOrder(order);
        }

        private async Task SaveOrder(IRegisterOrderCommand result)
        {
            var order = new Order
            {
                OrderId = result.OrderId,
                UserEmail = result.UserEmail,
                Status = Status.Registered,
                PictureUrl = result.PictureUrl,
                ImageData = result.ImageData
            };
            await _orderRepo.RegisterOrder(order);
        }
    }
}
