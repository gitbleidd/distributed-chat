using Chat.Server.Hubs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;

namespace Chat.Server.RabbitMq
{
    public class RabbitMqConsumerService
    {
        private readonly RabbitMqService _rabbitMqService;
        private readonly IModel _channel;
        private readonly AsyncEventingBasicConsumer _consumer;

        public RabbitMqConsumerService(RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
            _channel = rabbitMqService.Channel;
            _consumer = new AsyncEventingBasicConsumer(_channel);
        }

        public void AddReceivedEventHandler(AsyncEventHandler<BasicDeliverEventArgs> onConsumerOnReceived)
        {
            _consumer.Received += onConsumerOnReceived;
            _channel.BasicConsume(queue: _rabbitMqService.QueueName,
                autoAck: true,
                consumer: _consumer);
        }
        
        public void SendMessage(ChatMqMessage message)
        {
            var serializedMessage = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(serializedMessage);

            _channel.BasicPublish(exchange: "amq.fanout",
                           routingKey: "",
                           basicProperties: null,
                           body: body);
        }
    }
}
