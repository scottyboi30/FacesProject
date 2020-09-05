﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Faces.WebMvc.Models;
using Refit;

namespace Faces.WebMvc.RestClients
{
    public interface IOrderApi
    {
        [Get("/orders")]
        Task<List<OrderViewModel>> GetOrders();

        [Get("/orders/{orderId}")]
        Task<OrderViewModel> GetOrderById(Guid orderId);
    }
}
