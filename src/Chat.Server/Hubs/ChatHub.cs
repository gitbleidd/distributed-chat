using Chat.Server.Data;
using Chat.Server.Data.Entities;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatContext _context;
        public ChatHub(ChatContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string user, string message)
        {
            // 1. Save in DB
            _context.Add(new Message { User = user, Content = message });

            // 2. Send to other servers via RabbitMQ
            // TODO
            
            // 3. Send to other clients
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
