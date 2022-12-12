using Grpc.Net.Client;
using Chat.Server.Hubs;
using Microsoft.EntityFrameworkCore;
using Chat.Server.Data;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Chat.Server;

namespace Chat.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Listen gRPC from chat servers and SignalR from clients
            //builder.Services.AddLogging();
            builder.Services.AddGrpc();
            builder.Services.AddSignalR((o) => o.EnableDetailedErrors = true);

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ChatContext>(options => options.UseNpgsql(connectionString));
            //builder.Services.AddDbContext<ChatContext>(options => options.UseInMemoryDatabase("distributed_chat"));
            builder.Services.AddSingleton(new Users());

            var app = builder.Build();
            app.MapGrpcService<GrpcServices.InteractionService>();
            app.UseRouting();

            var chatHubPath = builder.Configuration["HubPaths:ChatHub"];
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>(chatHubPath);
            });

            app.Start();


            // Ping dispatcher to send ports
            var server = app.Services.GetService<IServer>()!;
            var addressFeature = server.Features.Get<IServerAddressesFeature>()!;

            string port1 = string.Empty; 
            string port2 = string.Empty;
            if (addressFeature.Addresses.Count == 2)
            {
                port1 = GetPortFromUrl(addressFeature.Addresses.ElementAt(0));
                port2 = GetPortFromUrl(addressFeature.Addresses.ElementAt(1));
            }

            if (string.IsNullOrEmpty(port1) || string.IsNullOrEmpty(port2))
            {
                Console.WriteLine("Couldn't get two ports (HTTP/1.1, HTTP/2.0) kestrel is running on");
                await app.StopAsync();
                Console.ReadKey();
                return;
            }

            var dispatcherAddress = builder.Configuration["DispatcherAddress"];
            if (!await PingDispatcher(dispatcherAddress, port1, port2))
            {
                Console.WriteLine("Error: couldn't connect to dispatcher");
                Console.ReadKey();
                return;
            }
            
            app.WaitForShutdown();
        }

        public static async Task<bool> PingDispatcher(string dispatcherAddress, string port1, string port2)
        {
            using var channel = GrpcChannel.ForAddress(dispatcherAddress);
            var client = new Rpc.Core.ServiceInteraction.ServiceInteractionClient(channel);

            int attemptCount = 3;
            for (int i = 0; i < attemptCount; i++)
            {
                Console.WriteLine($"Ping attempt ({i}).");
                try
                {
                    var pingResult = await client.PingAsync(
                        new Rpc.Core.PingMessage 
                        { 
                            Port1 = port1, 
                            Port2 = port2
                        }, 
                        deadline: DateTime.UtcNow.AddSeconds(5)
                    );

                    await channel.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5.0));
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");
                    Thread.Sleep(5000);
                }
            }

            return false;
        }

        public static string GetPortFromUrl(string url)
        {
            int firstColonIndex = url.IndexOf(':');
            if (firstColonIndex == -1) return string.Empty;

            int portColonIndex = url.IndexOf(':', firstColonIndex + 1);
            if (portColonIndex == -1) return string.Empty;

            return url[(portColonIndex + 1)..];
        }
    }
}