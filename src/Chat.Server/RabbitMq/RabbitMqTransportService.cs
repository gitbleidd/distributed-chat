using Chat.Server.Hubs;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Chat.Server.RabbitMq
{
    public class RabbitMqTransportService
    {
        private readonly RabbitMqService _rabbitMqService;
        private static IModel _channel;

        public RabbitMqTransportService(IConfiguration configuration, RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
            _channel = rabbitMqService.Channel;
        }

        public void SendMessage(ChatMqMessage message)
        {
            var serializedMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(serializedMessage);

            _channel.BasicPublish(exchange: "amq.fanout",
                           routingKey: "",
                           basicProperties: null,
                           body: body);
        }
    }
        
}
