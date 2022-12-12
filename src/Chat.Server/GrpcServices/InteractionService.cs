using Chat.Server;
using Grpc.Core;
using Rpc.Core;

namespace Chat.Server.GrpcServices
{
    public class InteractionService : Rpc.Core.ServiceInteraction.ServiceInteractionBase
    {
        public readonly Users _users;
        public InteractionService(Users users)
        {
            _users = users;
        }

        public override Task<PingReply> Ping(PingMessage request, ServerCallContext context)
        {
            return Task.FromResult(new PingReply { IsSuccessful = true });
        }

        public override Task<GetConnectionInfoReply> GetConnectionInfo(LoginMessage request, ServerCallContext context)
        {
            // TODO в словаре храню в качестве ключа connection ID, а не имя пользователя.

            bool isConnected = false;
            foreach (var user in _users.Logins)
            {
                if (user.Value == request.Login)
                {
                    isConnected = true;
                    break;
                }
            }

            var result = new GetConnectionInfoReply
            {
                ClientsNumber = _users.Logins.Count,
                IsConnected = isConnected
            };

            return Task.FromResult(result);
        }
    }
}
