using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Chat.Server
{
    public static class ServerInitUtils
    {
        public static async Task<bool> SendPortsToDispatcher(ILogger logger, string dispatcherAddress, string http2Port, string http1Port, int attemptCount = 3)
        {
            using var channel = GrpcChannel.ForAddress(dispatcherAddress);
            var client = new Rpc.Core.ServiceInteraction.ServiceInteractionClient(channel);
            
            for (int i = 0; i < attemptCount; i++)
            {
                logger.LogInformation($"Ping attempt ({i}).");
                try
                {
                    var pingResult = await client.PingAsync(
                        new Rpc.Core.PingMessage 
                        { 
                            Port1 = http2Port,
                            Port2 = http1Port
                        }, 
                        deadline: DateTime.UtcNow.AddSeconds(5)
                    );

                    await channel.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5.0));
                    return true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    Thread.Sleep(5000);
                }
            }
            
            return false;
        }

        public static List<string> GetServerPorts(IServiceProvider services)
        {
            IServer server = services.GetService<IServer>()!;
            var addressFeature = server.Features.Get<IServerAddressesFeature>()!;
            return addressFeature.Addresses.ToList();
        }

        private static string GetPortFromUrl(string url)
        {
            int firstColonIndex = url.IndexOf(':');
            if (firstColonIndex == -1) 
                return string.Empty;
            
            int portColonIndex = url.IndexOf(':', firstColonIndex + 1);
            if (portColonIndex == -1) 
                return string.Empty;
            
            return url[(portColonIndex + 1)..];
        }
    }
}