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

        private static Dictionary<string, GRPCClient> _clients = new();

        private static string netcodeServerIP = "";
        public static NetcodeServer NetcodeServer 
        {
            get
            {
                if (!_clients.ContainsKey(netcodeServerIP))
                {
                    //Can't use logger since this is static
                    Console.WriteLine("Trying to get NetcodeServer but it's not registered yet.");
                    return null!;
                }

                return (_clients[netcodeServerIP] as NetcodeServer)!;
            }
        }

        public static void DisconnectClient(string clientAdress)
        {
            if(!_clients.TryGetValue(clientAdress, out var cli)) 
            {
                //Can't use logger since this is static
                Console.WriteLine("Trying to disconnect a client that is not connected. IP: " + clientAdress);
                return; 
            }
            
            cli.Disconnect();
            _clients.Remove(clientAdress);
            
            Console.WriteLine($"Disconnect {clientAdress}\n");

            //Debug connected clients
            foreach (var item in _clients)
            {
                Console.WriteLine($"{item.Key}, {item.Value}");
            }
            Console.WriteLine();
        }

        #endregion

        #region Handshake

        public override Task<GRPC_HandshakeGet> GRPC_Handshake(GRPC_HandshakePost request, ServerCallContext context)
        {            
            //Debug
            Console.WriteLine("> Handshake");
            Console.WriteLine(context.Host);
            Console.WriteLine(context.Peer + "\n");

            if (_clients.Count > 0)
            {
                _clients.Add(context.Peer, new UnrealClient(ad: context.Peer));

                //Debug connected clients
                foreach (var item in _clients)
                {
                    Console.WriteLine($"{item.Key}, {item.Value}");
                }
                Console.WriteLine();

                return Task.FromResult(new GRPC_HandshakeGet
                {
                    Result = 0, //0 = good!
                    //Send netobjects
                });
            }
            else
            {
                _logger.LogCritical("Getting Handshake, but there is no NetcodeServer. " +
                    "Connect UnrealClients after NetcodeServer!");

                //Result != 0 => error
                return Task.FromResult(new GRPC_HandshakeGet { Result = 1 });
            }

        }

        public override Task<GRPC_NHandshakeGet> GRPC_NetcodeHandshake(GRPC_NHandshakePost request, ServerCallContext context)
        {
            //Debug
            Console.WriteLine("> NetcodeHandshake");
            Console.WriteLine(context.Host);
            Console.WriteLine(context.Peer + "\n");

            if (NetcodeServer == null)
            {
                var adress = context.Peer;
                netcodeServerIP = adress;
                _clients.Add(adress, new NetcodeServer(adress));

                //Debug connected clients
                foreach (var item in _clients)
                {
                    Console.WriteLine($"{item.Key}, {item.Value}");
                }
                Console.WriteLine();

                return Task.FromResult(new GRPC_NHandshakeGet { Result = 0 });
            }
            
            _logger.LogCritical("Getting NetcodeHandshake, but there is already an active NetcodeServer!");

            return Task.FromResult(new GRPC_NHandshakeGet { Result = 1 });
        }

        #endregion

        #region Ping

        public override async Task GRPC_Ping(IAsyncStreamReader<GRPC_PingPost> requestStream, 
            IServerStreamWriter<GRPC_PingGet> responseStream, ServerCallContext context)
        {
            GRPC_PingGet empty = new();

            try
            {
                while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
                {
                    await responseStream.WriteAsync(empty);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Connection lost with client.");
                DisconnectClient(context.Peer);
            }
        }

        #endregion

        #region NetObjects Update



        #endregion
    }
}