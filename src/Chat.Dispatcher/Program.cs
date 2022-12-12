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
            //app.MapGet("/", () => "Hello World!").RequireHost($"*:25566");

            app.Run();
        }
    }
}