using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Numerics;
using Utils;

namespace GRPCServer.Services
{
    public class MainServiceImpl : MainService.MainServiceBase
    {
        private readonly ILogger<MainServiceImpl> _logger;
        public MainServiceImpl(ILogger<MainServiceImpl> logger)
        {
            _logger = logger;
        }

        #region Handshake

        public override Task<HandshakeGet> Handshake(HandshakePost request, ServerCallContext context)
        {
            return Task.FromResult(new HandshakeGet
            {
            });
        }

        public override Task<NHandshakeGet> NetcodeHandshake(NHandshakePost request, ServerCallContext context)
        {
            return Task.FromResult(new NHandshakeGet());
        }

        #endregion

        #region Ping

        public override async Task Ping(IAsyncStreamReader<PingPost> requestStream, IServerStreamWriter<PingGet> responseStream, ServerCallContext context)
        {
            PingGet empty = new();

            await Streaming.SafeStream(async () => {
                while (await requestStream.MoveNext())
                {
                    await responseStream.WriteAsync(empty);
                }
            });
        }

        #endregion
    }
}