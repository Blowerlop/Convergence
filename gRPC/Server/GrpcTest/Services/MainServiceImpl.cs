using Grpc.Core;
using Microsoft.AspNetCore.ResponseCompression;
using Networking;
using Utils;

namespace GRPCServer.Services
{
    public struct NetcodeServerWrapper
    {
        public string adress { get; private set; }
        public NetcodeServer netcodeServer { get; private set; }

        public NetcodeServerWrapper(string serverIp, NetcodeServer netcodeServer)
        {
            this.adress = serverIp;
            this.netcodeServer = netcodeServer;
        }
    }

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

        public static readonly Dictionary<string, GRPCClient> clients = new();
        public static readonly Dictionary<string, UnrealClient> unrealClients = new Dictionary<string, UnrealClient>();
        public static NetcodeServerWrapper? netcodeServer  = null;

        private void AddNetcodeServer(string ip, string ad)
        {
            if (netcodeServer != null && netcodeServer.HasValue)
            {
                Console.WriteLine("Netcode server already connected. IP: " + ip);
                return;
            }


            NetcodeServerWrapper tempNetcodeServer = new NetcodeServerWrapper(ip, new NetcodeServer(ad));
            if (clients.TryAdd(ip, tempNetcodeServer.netcodeServer) == false)
            {
                Console.WriteLine("Trying to connect a client that is already connected but not as Netcode server. IP: " + ip);
                return;
            }

            netcodeServer = tempNetcodeServer;
        }

        private void AddUnrealClient(string ip, string ad)
        {
            if (unrealClients.ContainsKey(ip))
            {
                Console.WriteLine("Unreal client already connected. IP: " + ip);
                return;
            }
            if (clients.ContainsKey(ip))
            {
                Console.WriteLine("Trying to connect a client that is already connected but not as unreal client. IP: " + ip);
                return;
            }

            UnrealClient unrealClient = new UnrealClient(ad);
            unrealClients.Add(ip, unrealClient);
            clients.Add(ip, unrealClient);
        }

        private void DisplayClients()
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine();

            Console.WriteLine($"Connected clients : {clients.Keys.Count}");
            Console.WriteLine();

            // Display Unreal clients
            Console.WriteLine($"Unreal clients : {unrealClients.Keys.Count}");
            foreach (KeyValuePair<string, UnrealClient> unrealClient in unrealClients)
            {
                Console.WriteLine(unrealClient.Value.Adress.ToString());
            }

            Console.WriteLine();
            Console.WriteLine();

            // Display NetcodeServer
            Console.WriteLine($"NetcodeServer :");
            if (netcodeServer != null && netcodeServer.HasValue)
            {
                Console.WriteLine(netcodeServer.Value.adress);
            }
            else
            {
                Console.WriteLine("Not connected");
            }
            Console.WriteLine();

            Console.WriteLine("-----------------------------------");
            Console.WriteLine();
        }




        public void DisconnectClient(string clientAdress)
        {
            if(!clients.TryGetValue(clientAdress, out var client)) 
            {
                //Can't use logger since this is static
                Console.WriteLine("Trying to disconnect a client that is not connected. IP: " + clientAdress);
                return; 
            }
            
            client.Disconnect();
            
            Console.WriteLine($"Disconnect {clientAdress}\n");

            DisplayClients();
            
        }

        #endregion

        #region Handshake

        public override Task<GRPC_HandshakeGet> GRPC_Handshake(GRPC_HandshakePost request, ServerCallContext context)
        {            
            //Debug
            Console.WriteLine("> Handshake");
            Console.WriteLine(context.Host);
            Console.WriteLine(context.Peer + "\n");

            if (clients.Count > 0)
            {
                // Catch if client is already added to the clients
                string clientAdress = context.Peer;
                AddUnrealClient(clientAdress, clientAdress);

                DisplayClients();

                return Task.FromResult(new GRPC_HandshakeGet
                {
                    Result = 0, //0 = good!
                    ClientId = clients.Keys.Count + 1
                    //Send netobjects
                });
            }
            else
            {
                _logger.LogCritical("Getting Handshake, but there is no NetcodeServer. " +
                    "Connect UnrealClients after NetcodeServer!");

                //Result != 0 => error
                return Task.FromResult(new GRPC_HandshakeGet { Result = 1, ClientId = -1 });
            }

        }

        public override Task<GRPC_NHandshakeGet> GRPC_NetcodeHandshake(GRPC_NHandshakePost request, ServerCallContext context)
        {
            //Debug
            Console.WriteLine("> NetcodeHandshake");
            Console.WriteLine(context.Host);
            Console.WriteLine(context.Peer + "\n");

            if (netcodeServer.HasValue == false)
            {
                string adress = context.Peer;
                AddNetcodeServer(adress, adress);

                DisplayClients();

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

        public override async Task<GRPC_EmptyMsg> GRPC_SrvNetVarUpdate(IAsyncStreamReader<GRPC_NetVarUpdate> requestStream, ServerCallContext context)
        {
            Console.WriteLine($"Wittring stream opened");
            try
            {
                while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"NetVar received {requestStream.Current.HashName}");
                    foreach (KeyValuePair<string, UnrealClient> unrealClient in unrealClients)
                    {
                        //await unrealClient.Value._netVarStream?.WriteAsync(requestStream.Current);

                        if (unrealClient.Value.netVarStream.ContainsKey(requestStream.Current.HashName) == false) continue;

                        await unrealClient.Value.netVarStream[requestStream.Current.HashName].WriteAsync(requestStream.Current);
                    }
                    
                    Console.WriteLine($"NetVar sync {requestStream.Current.HashName}");
                }
            }
            catch (IOException)
            {
                _logger.LogCritical("Connection lost with client.");
                Console.WriteLine($"stream closed");
                DisconnectClient(context.Peer);
                return new GRPC_EmptyMsg();
            }

            Console.WriteLine($"Wittring stream closed");
            DisconnectClient(context.Peer);
            return new GRPC_EmptyMsg();
        }

        public override async Task GRPC_CliNetNetVarUpdate(GRPC_NetVarUpdate request, IServerStreamWriter<GRPC_NetVarUpdate> responseStream, ServerCallContext context)
        {
            Console.WriteLine($"Response stream opened");

            //unrealClients[context.Peer]._netVarStream = responseStream;
            unrealClients[context.Peer].netVarStream.Add(request.HashName, responseStream);

            try
            {
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                unrealClients[context.Peer].netVarStream.Remove(request.HashName);
            }
            Console.WriteLine($"Response stream closed");
        }

        #endregion
    }
}