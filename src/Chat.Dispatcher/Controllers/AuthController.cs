using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using static Chat.Dispatcher.SerializeUtils;

namespace Chat.Dispatcher.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ChatServerAddresses _serverAddresses;
        private readonly string _serverAddressesFilePath;

        public AuthController(ILogger<AuthController> logger, ChatServerAddresses serverAddresses)
        {
            _logger = logger;
            _serverAddresses = serverAddresses;
            _serverAddressesFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.ServerAddressesFileName);
        }

        /// <summary>
        /// Returns all running servers addresses.
        /// </summary>
        [HttpGet]
        public ActionResult<string> GetAddresses()
        {
            var sb = new StringBuilder("(✿◕‿◕✿)");
            foreach (var item in _serverAddresses.Addresses)
            {
                sb.AppendLine($"{item.Key}: (HTTP/1.1 {item.Value.Http1Port}), (HTTP/2.0 {item.Value.Http2Port})");
            }

            return Ok(sb.ToString());
        }

        /// <summary>
        /// Returns server address for non-authed user or
        /// error if user with this name already exists or
        /// empty string if couldn't find any server.
        /// </summary>
        /// <param name="authRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] AuthRequest authRequest)
        {
            // Asking all servers - has this user been already authed.
            // Also choosing server with minimal user count.

            int minCount = int.MaxValue;
            ChatServerAddressInfo? bestAddress = null;

            var addressesToRemove = new List<string>();

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
                    addressesToRemove.Add(address.Key);
                }
            }

            // Remove shutdown server addresses
            foreach (var key in addressesToRemove)
            {
                _serverAddresses.Addresses.Remove(key);
            }

            // Save new list of addresses to file
            if (!SerializeToFile(_serverAddresses, _serverAddressesFilePath))
            {
                _logger.LogError($"Couldn't save {Constants.ServerAddressesFileName} file");
            }

            // All servers are shutdown
            if (bestAddress == null)
                return Ok(new { address = $"" });

            // Got best address
            return Ok(new { address = $"{bestAddress.Ip}:{bestAddress.Http1Port}" });
        }
    }
}
