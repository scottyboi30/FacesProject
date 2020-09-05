using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Faces.WebMvc.Models;
using Microsoft.Extensions.Configuration;
using Refit;

namespace Faces.WebMvc.RestClients
{
    public class OrderApi : IOrderApi
    {
        private readonly IOrderApi _restClient;

        public OrderApi(IConfiguration config, HttpClient httpClient)
        {
            var apiHostAndPort = config.GetSection("ApiServiceLocations").
                GetValue<string>("OrdersApiLocation");

            httpClient.BaseAddress = new Uri($"http://{apiHostAndPort}/api");

            _restClient = RestService.For<IOrderApi>(httpClient);
        }

        public async Task<OrderViewModel> GetOrderById(Guid orderId)
        {
            try
            {
                return await _restClient.GetOrderById(orderId);
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public async Task<List<OrderViewModel>> GetOrders()
        {
            return await _restClient.GetOrders();
        }
    }
}
