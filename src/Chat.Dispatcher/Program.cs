using Chat.Dispatcher.Controllers;
using static Chat.Dispatcher.SerializeUtils;

namespace Chat.Dispatcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            
            builder.Services.AddGrpc();
            
            builder.Services.AddSingleton(GetChatServerAddresses());
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

        private static ChatServerAddresses GetChatServerAddresses()
        {
            const string serverAddressesFileName = Constants.ServerAddressesFileName;
            string serverAddressesFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, serverAddressesFileName);
            var configServerAddresses = TryDeserializeFile<ChatServerAddresses>(serverAddressesFilePath);
            
            var result = new ChatServerAddresses();
            if (configServerAddresses == null)
            {
                Console.WriteLine($"Error: couldn't read {serverAddressesFileName} file.");
                return result;
            }
            
            foreach (var address in configServerAddresses.Addresses)
            {
                result.Addresses.TryAdd(address.Key, address.Value);
            }
            return result;
        }
    }
}