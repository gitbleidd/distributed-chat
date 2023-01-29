using Chat.Server.Hubs;
using Microsoft.EntityFrameworkCore;
using Chat.Server.Data;
using Chat.Server.RabbitMq;


namespace Chat.Server
{
    public class Program
    {
        private static readonly ILogger<Program> Logger;
        static Program()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();                
            });
            Logger = loggerFactory.CreateLogger<Program>();
        }

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // Listen gRPC from chat servers and SignalR from clients
            builder.Services.AddGrpc();
            builder.Services.AddSignalR((o) => o.EnableDetailedErrors = true);

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ChatContext>(options => options.UseNpgsql(connectionString));
            builder.Services.AddTransient<MessageHandler>();
            builder.Services.AddSingleton(new UsersStorage());
            builder.Services.AddSingleton<RabbitMqWrapper>();
            builder.Services.AddSingleton<RabbitMqPublisher>();
            
            var app = builder.Build();

            app.MapGrpcService<GrpcServices.InteractionService>();
            app.UseRouting();

            var chatHubPath = builder.Configuration["HubPaths:ChatHub"];
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>(chatHubPath);
            });
            
            app.Start();
            var rabbitMqWrapper = app.Services.GetRequiredService<RabbitMqWrapper>();
            rabbitMqWrapper.Consumer.Received += async (sender, @event) =>
            {
                try
                {
                    var messageHandler = app.Services.GetRequiredService<MessageHandler>();
                    await messageHandler.Handle(sender, @event);
                }
                catch (Exception e)
                {
                    Logger.LogError(e.Message);
                }
            };

            // Sending server http1 and http2 ports to dispatcher or else
            // shutting down server.
            string dispatcherAddress = builder.Configuration["DispatcherAddress"];
            var ports = ServerInitUtils.GetServerPorts(app.Services);
            if (ports.Count != 2)
            {
                Logger.LogError("Couldn't get two ports (HTTP/1.1, HTTP/2.0) kestrel is running on");
                await app.StopAsync();
                Console.ReadKey();
            }
            if (!await ServerInitUtils.SendPortsToDispatcher(Logger, dispatcherAddress, ports.First(), ports.Skip(1).First()))
            {
                Logger.LogError("Couldn't connect to dispatcher");
                await app.StopAsync();
                Console.ReadKey();
            }
            Logger.LogInformation($"Ports successfully sent to Dispatcher");
            
            await app.WaitForShutdownAsync();
        }
    }
}