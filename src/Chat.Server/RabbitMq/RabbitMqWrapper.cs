using System.Text;
using Chat.Server.Hubs;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.RabbitMq
{
    public class RabbitMqWrapper
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        public IModel Channel { get; }
        private string QueueName { get; }
        public AsyncEventingBasicConsumer Consumer { get; }

        public RabbitMqWrapper(IConfiguration configuration)
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

            QueueName = Guid.NewGuid().ToString();
            Channel.QueueDeclare(queue: QueueName,
                durable: true,
                exclusive: true,
                autoDelete: true,
                arguments: null);

            Channel.QueueBind(queue: QueueName,
                exchange: "amq.fanout",
                routingKey: "");
            
            Consumer = new AsyncEventingBasicConsumer(Channel);

            Channel.BasicConsume(queue: QueueName,
                autoAck: true,
                consumer: Consumer);
        }
    }
}

