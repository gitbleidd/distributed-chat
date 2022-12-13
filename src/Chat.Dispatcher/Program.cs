using Chat.Dispatcher.Controllers;

namespace Chat.Dispatcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddGrpc();
            //builder.Services.AddControllers();
            
            var serverAddressesFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server_addresses.json");
            var s = Utils.Deserialize<ServerAddresses>(serverAddressesFilePath);
            var serverAddresses = new ServerAddresses();
            if (s == null)
            {
                Console.WriteLine($"Error: couldn't read server_addresses file.");
            }
            else
            {
                foreach (var address in s.Addresses)
                {
                    serverAddresses.Addresses.TryAdd(address.Key, address.Value);
                }
            }
            
            builder.Services.AddSingleton(new ServerAddresses());

            builder.Services.AddSingleton<AuthController, AuthController>();
            builder.Services.AddMvc().AddControllersAsServices();

            var app = builder.Build();
            app.MapGrpcService<GrpcServices.InteractionService>().RequireHost($"*:25565");

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Run();
        }
    }
}