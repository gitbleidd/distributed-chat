using Grpc.Core;
using Rpc.Core;

namespace Chat.Server.GrpcServices
{
    public class InteractionService : Rpc.Core.ServiceInteraction.ServiceInteractionBase
    {
        private readonly Users _users; // Current server list of users
        public InteractionService(Users users)
        {
            _users = users;
        }

        public override Task<PingReply> Ping(PingMessage request, ServerCallContext context)
        {
            return Task.FromResult(new PingReply { IsSuccessful = true });
        }

        /// <summary>
        /// Returns is user connect to current server and users count. 
        /// </summary>
        public override Task<GetConnectionInfoReply> GetConnectionInfo(LoginMessage request, ServerCallContext context)
        {
            bool isConnected = _users.Logins.Any(user => user.Value == request.Login);

            var result = new GetConnectionInfoReply
            {
                ClientsNumber = _users.Logins.Count,
                IsConnected = isConnected
            };

            return Task.FromResult(result);
        }
    }
}
