using System.Text;
using Chat.Server.Hubs;
using Chat.Server.RabbitMq;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace Chat.Server;

public class MessageHandler
{
    private readonly UsersStorage _users;
    private readonly IHubContext<ChatHub> _chatHub;

    public MessageHandler(UsersStorage users, IHubContext<ChatHub> chatHub)
    {
        _users = users;
        _chatHub = chatHub;
    }
    
    public async Task Handle(object model, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);

        var chatMqMessage = JsonConvert.DeserializeObject<ChatMqMessage>(message);
        if (chatMqMessage == null) return;

        string sessionId = string.Empty;
        foreach (KeyValuePair<string, string> serverUsername in _users.Logins)
        {
            if (serverUsername.Value != chatMqMessage.Username) continue;
            sessionId = serverUsername.Key;
            break;
        }

        if (string.IsNullOrEmpty(sessionId))
        {
            await _chatHub.Clients.AllExcept(sessionId).SendAsync(
                "ReceiveMessage", chatMqMessage.Username, chatMqMessage.Content);
        }
    }
}