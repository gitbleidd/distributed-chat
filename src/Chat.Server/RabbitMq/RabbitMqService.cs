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
        private static Users _users;


        // TODO игнорировать себя!
        public RabbitMqService(IConfiguration configuration, IServiceProvider provider, IHubContext<ChatHub> hubContext, Users users)
        {
            _configuration = configuration;
            _users = users;

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

                    string sessionId = string.Empty;
                    foreach (KeyValuePair<string, string> username in _users.Logins)
                    {
                        if (username.Value == chatMqMessage.Username)
                        {
                            sessionId = username.Key;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(sessionId))
                        await hubContext.Clients.AllExcept(sessionId).SendAsync("ReceiveMessage", chatMqMessage.Username, chatMqMessage.Content);
                        
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
