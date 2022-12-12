using Grpc.Net.Client;
using Chat.Server.Hubs;
using Microsoft.EntityFrameworkCore;
using Chat.Server.Data;

namespace Chat.Dispatcher
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Ping dispatcher before start
            var dispatcherAddress = builder.Configuration["DispatcherAddress"];
            if (!await PingDispatcher(dispatcherAddress)) {
                Console.WriteLine("Error: couldn't connect to dispatcher");
                return;
            }

            // 2. Listen gRPC from chat servers and SignalR from clients
            //builder.Services.AddLogging();
            builder.Services.AddGrpc();
            builder.Services.AddSignalR((o) => o.EnableDetailedErrors = true);

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            //builder.Services.AddDbContext<ChatContext>(options => options.UseNpgsql(connectionString));
            builder.Services.AddDbContext<ChatContext>(options => options.UseInMemoryDatabase("distributed_chat"));

            var app = builder.Build();

            app.MapGrpcService<GrpcServices.InteractionService>();
            app.UseRouting();

            var chatHubPath = builder.Configuration["HubPaths:ChatHub"];
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>(chatHubPath);
            });

            app.Run();
        }

        public static async Task<bool> PingDispatcher(string dispatcherAddress)
        {
            using var channel = GrpcChannel.ForAddress(dispatcherAddress);
            var client = new Rpc.Core.ServiceInteraction.ServiceInteractionClient(channel);

            int attemptCount = 5;
            for (int i = 0; i < attemptCount; i++)
            {
                Console.WriteLine($"Ping attempt ({i}).");
                try
                {
                    var pingResult = client.Ping(new Rpc.Core.EmptyMessage());
                    await channel.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5.0));
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");
                    Thread.Sleep(1000);
                }
            }

            return false;
        }
    }
}