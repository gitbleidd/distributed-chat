namespace Chat.Server.RabbitMq
{
    public class ChatMqMessage
    {
        public string Username { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
