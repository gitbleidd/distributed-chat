﻿using Chat.Server.Data;
using Chat.Server.Data.Entities;
using Chat.Server.RabbitMq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chat.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatContext _context;
        private readonly UsersStorage _users;
        private readonly RabbitMqPublisher _rabbitMq;

        public ChatHub(ChatContext context, UsersStorage users, RabbitMqPublisher mqConsumer)
        {
            _context = context;
            _users = users;
            _rabbitMq = mqConsumer;
        }

        public async Task SendMessage(string user, string message)
        {
            // 1. Save in DB
            _context.Messages.Add(new Message { User = user, Content = message });
            await _context.SaveChangesAsync();

            // 2. Send to other servers via RabbitMQ
            _rabbitMq.SendMessage(new ChatMqMessage { Username = user, Content = message });

            // 3. Send to other clients
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task GetHistory()
        {
            var messages = await _context.Messages.OrderByDescending(p => p.Id).Take(5).ToListAsync();
            await Clients.Caller.SendAsync("ReceiveHistory", messages);
        }

        public async Task Auth(string login)
        {
            _users.Logins.TryAdd(Context.ConnectionId, login);
            await Clients.Caller.SendAsync("ReceiveAuth", true);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _users.Logins.Remove(Context.ConnectionId, out string? value);

            return base.OnDisconnectedAsync(exception);
        }
    }
}
