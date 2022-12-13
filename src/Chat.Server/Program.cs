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
        //private static string _hostname = null!;
        //private static int _port;
        //private static string _username = null!;
        //private static string _password = null!;
        //private static ConnectionFactory _factory = null!;
        //private static RabbitMQ.Client.IConnection _connection = null!;
        //private static IModel _channel = null!;
        //private static AsyncEventingBasicConsumer _consumer = null!;
        //private static string _queueName = null!;
        //private static bool isQueueCreated = false;

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Listen gRPC from chat servers and SignalR from clients
            //builder.Services.AddLogging();
            builder.Services.AddGrpc();
            builder.Services.AddSignalR((o) => o.EnableDetailedErrors = true);

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ChatContext>(options => options.UseNpgsql(connectionString));
            builder.Services.AddSingleton(new Users());

            builder.Services.AddSingleton<RabbitMqService>();

            var app = builder.Build();

            #region RabbitMq
            var configuration = app.Configuration;
            var _hostname = configuration.GetValue<string>("RabbitMq:Hostname");
            var _port = configuration.GetValue<int>("RabbitMq:Port");
            var _username = configuration.GetValue<string>("RabbitMq:Username");
            var _password = configuration.GetValue<string>("RabbitMq:Password");

            var _factory = new ConnectionFactory()
            {
                HostName = _hostname,
                Port = _port,
                UserName = _username,
                Password = _password,
                DispatchConsumersAsync = true,
            };

            var _connection = _factory.CreateConnection();
            var _channel = _connection.CreateModel();

            string _queueName = (Guid.NewGuid()).ToString();
            _channel.QueueDeclare(queue: _queueName,
                               durable: true,
                               exclusive: true,
                               autoDelete: true,
                               arguments: null);

            _channel.QueueBind(queue: _queueName,
              exchange: "amq.fanout",
              routingKey: "");
            #endregion

            RabbitMqService.queueName = _queueName;
            RabbitMqService.channel = _channel;

            app.MapGrpcService<GrpcServices.InteractionService>();
            app.UseRouting();

            var chatHubPath = builder.Configuration["HubPaths:ChatHub"];
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>(chatHubPath);
            });

            var chatHub = app.Services.GetService<RabbitMqService>();

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
                    Console.WriteLine($"Ping - Ok!");
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

    public static class ServicesExtention
    {
        public static IServiceCollection AddLazyResolution(this IServiceCollection services)
        {
            return services.AddTransient(
                typeof(Lazy<>),
                typeof(LazilyResolved<>));
        }

        private class LazilyResolved<T> : Lazy<T>
        {
            public LazilyResolved(IServiceProvider serviceProvider)
                : base(serviceProvider.GetRequiredService<T>)
            {
            }
        }
    }
}