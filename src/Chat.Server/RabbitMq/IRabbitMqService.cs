namespace Chat.Server.RabbitMq
{
    public interface IRabbitMqService
    {
        void SendMessage(ChatMqMessage message);
    }
}
