﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Faces.EmailService;
using MassTransit;
using Messaging.InterfacesConstants.Events;

namespace Faces.NotificationService.Consumers
{
    public class OrderProcessedEventConsumer : IConsumer<IOrderProcessedEvent>
    {
        private readonly IEmailSender _emailSender;

        public OrderProcessedEventConsumer(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Consume(ConsumeContext<IOrderProcessedEvent> context)
        {
            try
            {
                var rootFolder =
                    AppContext.BaseDirectory.Substring(0,
                        AppContext.BaseDirectory.IndexOf("bin", StringComparison.Ordinal));
                var result = context.Message;
                var facesData = result.Faces;
                if (facesData.Count < 1)
                    await Console.Out.WriteLineAsync("No faces Detected");
                else
                    foreach (var image in facesData.Select(face => new MemoryStream(face)).Select(Image.FromStream))
                        image.Save($"{rootFolder}/Images/face{result.OrderId}.jpg", ImageFormat.Jpeg);
                // Here we will add the Email Sending code

                string[] mailAddress = { result.UserEmail };
                 await _emailSender.SendEmailAsync(new Message(mailAddress, "your order" + result.OrderId,
                    "From FacesAndFaces", facesData));
                await context.Publish<IOrderDispatchedEvent>(new
                {
                    context.Message.OrderId,
                    DispatchDateTime = DateTime.UtcNow
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
