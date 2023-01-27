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
        private static AsyncEventingBasicConsumer _consumer;
        private readonly Users _users;
        public IModel Channel { get; }

        public RabbitMqService(IConfiguration configuration, IHubContext<ChatHub> hubContext, Users users)
        {
            _users = users;
            
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

            string queueName = (Guid.NewGuid()).ToString();
            Channel.QueueDeclare(queue: queueName,
                durable: true,
                exclusive: true,
                autoDelete: true,
                arguments: null);

            Channel.QueueBind(queue: queueName,
                exchange: "amq.fanout",
                routingKey: "");
            
            _consumer = new AsyncEventingBasicConsumer(Channel);

            _consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var chatMqMessage = JsonConvert.DeserializeObject<ChatMqMessage>(message);
                if (chatMqMessage == null) 
                    return;

                string sessionId = string.Empty;
                foreach (KeyValuePair<string, string> serverUsername in _users.Logins)
                {
                    if (serverUsername.Value == chatMqMessage.Username)
                    {
                        sessionId = serverUsername.Key;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(sessionId))
                    await hubContext.Clients.AllExcept(sessionId).SendAsync("ReceiveMessage", chatMqMessage.Username, chatMqMessage.Content);
                        
            };

            Channel.BasicConsume(queue: queueName,
                autoAck: true,
                consumer: _consumer);
        }
    }
}

