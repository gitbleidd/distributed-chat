namespace Chat.Server.RabbitMq
{
    public class ChatMqMessage
    {
        public string Username { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}
