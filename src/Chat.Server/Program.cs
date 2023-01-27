using Grpc.Net.Client;
using Chat.Server.Hubs;
using Microsoft.EntityFrameworkCore;
using Chat.Server.Data;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Chat.Server;
using Chat.Server.RabbitMq;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace Chat.Server
{
    public class Program
    {
        private static readonly ILogger<Program> _logger;
        static Program()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();                
            });
            _logger = loggerFactory.CreateLogger<Program>();
        }

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // Listen gRPC from chat servers and SignalR from clients
            //builder.Services.AddLogging();
            builder.Services.AddGrpc();
            builder.Services.AddSignalR((o) => o.EnableDetailedErrors = true);

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ChatContext>(options => options.UseNpgsql(connectionString));
            builder.Services.AddSingleton(new Users());

            builder.Services.AddSingleton<RabbitMqTransportService>();

            var app = builder.Build();

            app.MapGrpcService<GrpcServices.InteractionService>();
            app.UseRouting();

            var chatHubPath = builder.Configuration["HubPaths:ChatHub"];
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>(chatHubPath);
            });

            var chatHub = app.Services.GetService<RabbitMqTransportService>();

            app.Start();

            // Sending server http1 and http2 ports to dispatcher or else
            // shutting down server.
            string dispatcherAddress = builder.Configuration["DispatcherAddress"];
            var ports = ServerInitUtils.GetServerPorts(app.Services);
            if (ports.Count != 2)
            {
                _logger.LogError("Couldn't get two ports (HTTP/1.1, HTTP/2.0) kestrel is running on");
                await app.StopAsync();
                Console.ReadKey();
            }
            if (!await ServerInitUtils.SendPortsToDispatcher(_logger, dispatcherAddress, ports.First(), ports.Skip(1).First()))
            {
                _logger.LogError("Couldn't connect to dispatcher");
                await app.StopAsync();
                Console.ReadKey();
            }
            
            await app.WaitForShutdownAsync();
        }
    }
}