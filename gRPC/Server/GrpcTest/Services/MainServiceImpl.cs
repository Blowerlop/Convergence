using Grpc.Core;
using Networking;
using Utils;

namespace GRPCServer.Services
{
    public class MainServiceImpl : MainService.MainServiceBase
    {
        #region Init

        private readonly ILogger<MainServiceImpl> _logger;
        public MainServiceImpl(ILogger<MainServiceImpl> logger)
        {
            _logger = logger;
        }

        #endregion

        #region Refs

        //0 will always be NetcodeServer
        private List<GRPCClient> _clients = new();

        #endregion

        #region Handshake

        public override Task<HandshakeGet> Handshake(HandshakePost request, ServerCallContext context)
        {
            if (_clients.Count > 0)
                _clients.Add(new UnrealClient(id: _clients.Count));
            else
            {
                _logger.LogCritical("Getting Handshake, but there is no NetcodeServer. " +
                    "Connect UnrealClients after NetcodeServer!");

                return Task.FromResult(new HandshakeGet
                {
                    Result = 1 //1 = error, client will stop connecting
                });
            }


            return Task.FromResult(new HandshakeGet
            {
            });
        }

        public override Task<NHandshakeGet> NetcodeHandshake(NHandshakePost request, ServerCallContext context)
        {
            if (_clients.Count == 0)
                _clients.Add(new NetcodeServer());
            else if (_clients[0] is NetcodeServer)
                _logger.LogCritical("Getting NetcodeHandshake, but there is already an active NetcodeServer!");
            else
                _logger.LogCritical("Getting NetcodeHandshake, but there are already UnrealClients. " +
                    "This should not happen! Connect NetcodeServer first.");

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

        #region NetObjects Update



        #endregion
    }
}