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
        private static List<GRPCClient> _clients = new();

        public static NetcodeServer NetcodeServer 
        { 
            get 
            { 
                if (_clients.Count == 0)
                {
                    Console.WriteLine("Trying to get NetcodeServer but it's not registered yet.");
                    return null!;
                }

                return (_clients[0] as NetcodeServer)!;
            }
        }

        public static void DisconnectClient(int clientId)
        {
            if(clientId < 1 ||  clientId >= _clients.Count) { return; }

            DisconnectClient(_clients[clientId]);
        }

        public static void DisconnectClient(GRPCClient client)
        {
            client.Disconnect();
            _clients.Remove(client);
        }

        #endregion

        #region Handshake

        public override Task<HandshakeGet> Handshake(HandshakePost request, ServerCallContext context)
        {
            if (_clients.Count > 0)
            {
                int clientId = _clients.Count;
                _clients.Add(new UnrealClient(id: clientId));

                return Task.FromResult(new HandshakeGet
                {
                    Result = 0, //0 = good!
                    ClientId = clientId
                    //Send netobjects
                });
            }
            else
            {
                _logger.LogCritical("Getting Handshake, but there is no NetcodeServer. " +
                    "Connect UnrealClients after NetcodeServer!");

                //Result != 0 => error
                return Task.FromResult(new HandshakeGet { Result = 1 });
            }

        }

        public override Task<NHandshakeGet> NetcodeHandshake(NHandshakePost request, ServerCallContext context)
        {
            if (_clients.Count == 0)
            {
                _clients.Add(new NetcodeServer(id: 0));
                return Task.FromResult(new NHandshakeGet { Result = 0 });
            }
            else if (_clients[0] is NetcodeServer)
                _logger.LogCritical("Getting NetcodeHandshake, but there is already an active NetcodeServer!");
            else
                _logger.LogCritical("Getting NetcodeHandshake, but there are already UnrealClients. " +
                    "This should not happen! Connect NetcodeServer first.");

            return Task.FromResult(new NHandshakeGet { Result = 1 });
        }

        #endregion

        #region Ping

        public override async Task Ping(IAsyncStreamReader<PingPost> requestStream, IServerStreamWriter<PingGet> responseStream, ServerCallContext context)
        {
            var clientIdStr = context.RequestHeaders.First(x => x.Key == "client_id").Value;

            if (clientIdStr != null)
            {
                PingGet empty = new();

                int clientId = int.Parse(clientIdStr);

                try
                {
                    while (await requestStream.MoveNext())
                    {
                        await responseStream.WriteAsync(empty);
                    }
                }
                catch
                {
                    DisconnectClient(clientId);
                }

                //await Streaming.SafeStream(clientId, async () => {
                //    while (await requestStream.MoveNext())
                //    {
                //        await responseStream.WriteAsync(empty);
                //    }
                //});
            } 
        }

        #endregion

        #region NetObjects Update



        #endregion
    }
}