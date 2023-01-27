using Grpc.Core;
using Rpc.Core;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using static Chat.Dispatcher.SerializeUtils;

namespace Chat.Dispatcher.GrpcServices
{
    public class InteractionService : Rpc.Core.ServiceInteraction.ServiceInteractionBase
    {
        private readonly ChatServerAddresses _serverAddresses;
        private readonly string _serverAddressesFilePath;
        private readonly IConfiguration _configuration;
        private readonly bool _useHamachi;

        public InteractionService(ChatServerAddresses serverAddresses, IConfiguration configuration)
        {
            _serverAddressesFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.ServerAddressesFileName);
            _serverAddresses = serverAddresses;
            _configuration = configuration;
            _useHamachi = _configuration.GetValue<bool>("UseHamachi");
        }

        public override Task<Rpc.Core.PingReply> Ping(PingMessage request, ServerCallContext context)
        {
            // 1. Save address to list
            // Split 'ipv4:port' from Peer
            int firstColumnIndex = context.Peer.IndexOf(':');
            string fullAddress = context.Peer[(firstColumnIndex + 1)..];

            int portColumnIndex = fullAddress.IndexOf(':');
            string address = fullAddress[..portColumnIndex];

            if (address is "127.0.0.1" or "localhost")
            {
                string localAddress;
                if (_useHamachi)
                {
                    localAddress = AddressUtils.GetLocalAddressFromAdapter("Hamachi");
                }
                else
                {
                    localAddress = AddressUtils.GetLocalAddress();
                }

                if (string.IsNullOrEmpty(localAddress))
                    throw new Exception("Couldn't get local address");

                address = localAddress;
            }

            string grpcPort = fullAddress[(portColumnIndex + 1)..];

            var addressInfo = new ChatServerAddressInfo { 
                Ip = address, 
                Http2Port = request.Port1, 
                Http1Port = request.Port2,
                };

            _serverAddresses.Addresses.TryAdd($"{address}:{request.Port1}:{request.Port2}", addressInfo);

            // 2. Save address to file
            if (!SerializeToFile(_serverAddresses, _serverAddressesFilePath))
            {
                Console.WriteLine("Error: Couldn't save server_addresses file");
            }

            return Task.FromResult(new Rpc.Core.PingReply { IsSuccessful = true });
        }

    }
}
