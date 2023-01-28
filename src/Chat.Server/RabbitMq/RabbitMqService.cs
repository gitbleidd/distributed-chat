using System.Text;
using Chat.Server.Hubs;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.RabbitMq
{
    public class RabbitMqService
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly AsyncEventingBasicConsumer _consumer;
        
        public IModel Channel { get; }
        public string QueueName { get; }

        public RabbitMqService(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            var hostname = configuration.GetValue<string>("RabbitMq:Hostname");
            var port = configuration.GetValue<int>("RabbitMq:Port");
            var username = configuration.GetValue<string>("RabbitMq:Username");
            var password = configuration.GetValue<string>("RabbitMq:Password");

            _factory = new ConnectionFactory()
            {
                HostName = hostname,
                Port = port,
                UserName = username,
                Password = password,
                DispatchConsumersAsync = true,
            };

            _connection = _factory.CreateConnection();
            Channel = _connection.CreateModel();

            QueueName = (Guid.NewGuid()).ToString();
            Channel.QueueDeclare(queue: QueueName,
                durable: true,
                exclusive: true,
                autoDelete: true,
                arguments: null);

            Channel.QueueBind(queue: QueueName,
                exchange: "amq.fanout",
                routingKey: "");
            
            
            _consumer = new AsyncEventingBasicConsumer(Channel);
            async Task OnConsumerOnReceived(object model, BasicDeliverEventArgs ea)
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var chatMqMessage = JsonConvert.DeserializeObject<ChatMqMessage>(message);
                if (chatMqMessage == null) return;

                string sessionId = string.Empty;
                var users = serviceProvider.GetRequiredService<Users>();
                foreach (KeyValuePair<string, string> serverUsername in users.Logins)
                {
                    if (serverUsername.Value != chatMqMessage.Username) continue;
                    sessionId = serverUsername.Key;
                    break;
                }

                if (string.IsNullOrEmpty(sessionId))
                {
                    var hubContext = serviceProvider.GetRequiredService<IHubContext<ChatHub>>();
                    await hubContext.Clients.AllExcept(sessionId).SendAsync(
                        "ReceiveMessage", chatMqMessage.Username, chatMqMessage.Content);
                }
            }
            
            _consumer.Received += OnConsumerOnReceived;
            Channel.BasicConsume(queue: QueueName,
                autoAck: true,
                consumer: _consumer);
        }
    }
}

