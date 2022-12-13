using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Chat.Dispatcher.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ServerAddresses _serverAddresses;
        private readonly string _serverAddressesFilePath;

        public AuthController(ServerAddresses serverAddresses)
        {
            _serverAddresses = serverAddresses;
            _serverAddressesFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server_addresses.json");
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetAsync()
        {
            //await PostAsync(new AuthRequest { Login = "fasdf" });

            var sb = new StringBuilder();
            foreach (var item in _serverAddresses.Addresses)
            {
                sb.AppendLine($"{item.Key}: (HTTP/1.1 {item.Value.Http1Port}), (HTTP/2.0 {item.Value.Http2Port})");
            }

            return Ok("(✿◕‿◕✿)\n" + sb.ToString());
        }

        
        [HttpPost]
        public async Task<ActionResult<object>> PostAsync([FromBody] AuthRequest authRequest)
        {
            // Asking all servers - has this user been already authed.
            // Also choosing server with minimal users count

            int minCount = int.MaxValue;
            AddressInfo? bestAddress = null;

            var addressToRemove = new List<string>();

            foreach (var address in _serverAddresses.Addresses)
            {
                using var channel = GrpcChannel.ForAddress($"http://{address.Value.Ip}:{address.Value.Http2Port}");
                var client = new Rpc.Core.ServiceInteraction.ServiceInteractionClient(channel);
                try
                {
                    var info = await client.GetConnectionInfoAsync(
                        request: new Rpc.Core.LoginMessage { Login = authRequest.Login }, 
                        deadline: DateTime.UtcNow.AddSeconds(1)
                        );
                    if (info.IsConnected)
                    {
                        return Unauthorized(new { message = "User with this name has been already authed" });
                    }

                    if (info.ClientsNumber < minCount)
                    {
                        minCount = info.ClientsNumber;
                        bestAddress = address.Value;
                    }

                    await channel.ShutdownAsync().WaitAsync(TimeSpan.FromSeconds(5.0));
                }
                catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded)
                {
                    addressToRemove.Add(address.Key);
                }
                catch (Exception ex) { }
            }

            // Remove shutdown server addresses
            foreach (var key in addressToRemove)
            {
                _serverAddresses.Addresses.Remove(key);
            }

            // Save new list of addresses to file
            if (!Utils.Serialize(_serverAddresses, _serverAddressesFilePath))
            {
                Console.WriteLine("Error: Couldn't save server_addresses file");
            }

            // All servers are shutdown
            if (bestAddress == null)
                return Ok(new { address = $"" });

            // Got best address
            return Ok(new { address = $"{bestAddress.Ip}:{bestAddress.Http1Port}" });
        }
    }
}
