using Grpc.Core;
using Rpc.Core;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace Chat.Dispatcher.GrpcServices
{
    public class InteractionService : Rpc.Core.ServiceInteraction.ServiceInteractionBase
    {
        private readonly ServerAddresses _serverAddresses;
        private readonly string _serverAddressesFilePath;
        private readonly IConfiguration _configuration;
        private readonly bool _useHamachi;

        public InteractionService(ServerAddresses serverAddresses, IConfiguration configuration)
        {
            _serverAddressesFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server_addresses.json");
            _serverAddresses = serverAddresses;
            _configuration = configuration;
            _useHamachi = _configuration.GetValue<bool>("UseHamachi");

            var s = Utils.Deserialize<ServerAddresses>(_serverAddressesFilePath);
            if (s == null)
            {
                Console.WriteLine($"Error: couldn't read server_addresses file.");
            }
            else
            {
                foreach (var address in s.Addresses)
                {
                    _serverAddresses.Addresses.TryAdd(address.Key, address.Value);
                }
            }
        }

        public override Task<Rpc.Core.PingReply> Ping(PingMessage request, ServerCallContext context)
        {
            // 1. Save address to list
            // Split 'ipv4:port' from Peer
            int firstColumnIndex = context.Peer.IndexOf(':');
            string fullAddress = context.Peer[(firstColumnIndex + 1)..];

            int portColumnIndex = fullAddress.IndexOf(':');
            string address = fullAddress[..portColumnIndex];

            if (address == "127.0.0.1" || address == "localhost")
            {
                string localAddress;
                if (_useHamachi)
                {
                    localAddress = Utils.GetLocalAddressFromAdapter("Hamachi");
                }
                else
                {
                    localAddress = Utils.GetLocalAddress();
                }

                if (string.IsNullOrEmpty(localAddress))
                    throw new Exception("Couldn't get local address");

                address = localAddress;
            }

            string grpcPort = fullAddress[(portColumnIndex + 1)..];

            var addressInfo = new AddressInfo { 
                Ip = address, 
                Http2Port = request.Port1, 
                Http1Port = request.Port2,
                };

            _serverAddresses.Addresses.TryAdd($"{address}:{request.Port1}:{request.Port2}", addressInfo);

            // 2. Save address to file
            if (!Utils.Serialize(_serverAddresses, _serverAddressesFilePath))
            {
                Console.WriteLine("Error: Couldn't save server_addresses file");
            }

            return Task.FromResult(new Rpc.Core.PingReply { IsSuccessful = true });
        }

    }
}
