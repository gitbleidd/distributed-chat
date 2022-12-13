using Chat.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Chat.Server.RabbitMq
{
    public class RabbitMqService : IRabbitMqService
    {
        private IConfiguration _configuration;
        public static IModel channel = null!;
        public static string queueName = null!;
        private static bool isQueueCreated = false;
        private static AsyncEventingBasicConsumer _consumer = null!;


        // TODO игнорировать себя!
        public RabbitMqService(IConfiguration configuration, IServiceProvider provider, IHubContext<ChatHub> hubContext)
        {
            _configuration = configuration;

            if (!isQueueCreated)
            {
                _consumer = new AsyncEventingBasicConsumer(channel);

                _consumer.Received += async (model, ea) =>
                {
                    // Read message (encode, deserialize)
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    var chatMqMessage = JsonSerializer.Deserialize<ChatMqMessage>(message);
                    if (chatMqMessage == null)
                    {
                        return;
                    }

                    //Console.WriteLine($"\n---   {chatMqMessage.Username}:{chatMqMessage.Content}   ---\n");

                    //var chatHub = provider.GetService<ChatHub>();
                    //connectionManager.GetHubContext<ChatHub>();
                    //await chatHub.SendMessage(chatMqMessage.Username, chatMqMessage.Content);

                    await hubContext.Clients.All.SendAsync("ReceiveMessage", chatMqMessage.Username, chatMqMessage.Content);
                };

                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: _consumer);

                isQueueCreated = true;
            }
        }

        public void SendMessage(ChatMqMessage message)
        {
            var serializedMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(serializedMessage);

            channel.BasicPublish(exchange: "amq.fanout",
                           routingKey: "",
                           basicProperties: null,
                           body: body);
        }
    }
        
}
