using System;
using Faces.OrderApi.Data;
using Faces.OrderApi.Hubs;
using Faces.OrderApi.Messages.Consumers;
using Faces.OrderApi.Services;
using GreenPipes;
using MassTransit;
using Messaging.InterfacesConstants.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Faces.OrderApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<OrdersContext>(options => options.UseSqlServer
            (
                Configuration.GetConnectionString("OrdersContextConnection")
            ));

            services.AddHttpClient();

            services.AddSignalR()
                .AddJsonProtocol(options => { options.PayloadSerializerOptions.PropertyNamingPolicy = null; });
            
            services.AddTransient<IOrderRepository, OrderRepository>();

            services.AddMassTransit(
                c =>
                {
                    c.AddConsumer<RegisterOrderCommandConsumer>();
                    c.AddConsumer<OrderDispatchedEventConsumer>();
                });


            services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq((cfg) =>
            {
                cfg.Host("localhost", "/", h => { });
                cfg.ReceiveEndpoint(RabbitMqMassTransitConstants.RegisterOrderCommandQueue, e =>
                {
                    e.PrefetchCount = 16;//number of concurrent messages it can receive
                    e.UseMessageRetry(x => x.Interval(2, TimeSpan.FromSeconds(10)));
                    e.Consumer<RegisterOrderCommandConsumer>(provider);
                });

                cfg.ReceiveEndpoint(RabbitMqMassTransitConstants.OrderDispatchedServiceQueue, e =>
                {
                    e.PrefetchCount = 16;
                    e.UseMessageRetry(x => x.Interval(2, 100));
                    e.Consumer<OrderDispatchedEventConsumer>(provider);
                    //EndpointConvention.Map<OrderDispatchedEvent>(e.InputAddress);
                });

                //can add more endpoints. using the cfg.ReceiveEndpoint(.......
            }));

            services.AddSingleton<IHostedService, BusService>();
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed((host) => true)
                        .AllowCredentials());


            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<OrderHub>("/orderhub");
            });
        }
    }
}
