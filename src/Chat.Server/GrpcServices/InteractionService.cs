using Grpc.Core;
using Rpc.Core;

namespace Chat.Dispatcher.GrpcServices
{
    public class InteractionService : Rpc.Core.ServiceInteraction.ServiceInteractionBase
    {
        public override Task<PingReply> Ping(EmptyMessage request, ServerCallContext context)
        {
            return Task.FromResult(new PingReply { IsSuccessful = true });
        }

        public override Task<GetConnectionInfoReply> GetConnectionInfo(LoginMessage request, ServerCallContext context)
        {
            return Task.FromResult( new GetConnectionInfoReply {  ClientsNumber = Random.Shared.Next(1, 100), IsConnected = false });
        }
    }
}
