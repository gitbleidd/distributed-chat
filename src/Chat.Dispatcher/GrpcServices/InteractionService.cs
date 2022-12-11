using Grpc.Core;
using Rpc.Core;

namespace Chat.Dispatcher.GrpcServices
{
    public class InteractionService : Rpc.Core.ServiceInteraction.ServiceInteractionBase
    {
        private List<string> ServerHosts { get; }

        public InteractionService()
        {
            ServerHosts = new List<string>();
        }

        public override Task<PingReply> Ping(EmptyMessage request, ServerCallContext context)
        {
            // 1. Add to list
            // 2. Save to file on pc
            Console.WriteLine($"Server address: {context.Peer}");
            return Task.FromResult(new PingReply { IsSuccessful = true });
        }
    }
}
