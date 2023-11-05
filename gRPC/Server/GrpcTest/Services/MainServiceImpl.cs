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
        public static NetcodeServerWrapper? netcodeServer = null;

        public static event Action<UnrealClient> OnUnrealClientConnected;
        public static event Action<UnrealClient> OnUnrealClientDisconnected;
        
        private void AddNetcodeServer(string ip, string ad)
        {
            if (netcodeServer.HasValue)
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
            
            OnUnrealClientConnected?.Invoke(unrealClient);
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
            if (netcodeServer.HasValue)
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

            //Strange but cool way to cast then null check
            if(client as UnrealClient is { } unrealClient)
                OnUnrealClientDisconnected?.Invoke(unrealClient);
            
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
                    ClientId = clients.Keys.Count + 1,
                    
                    NetObjects = { netcodeServer?.netcodeServer.GetNetworkObjectsAsUpdates() },
                    NetVars = { netcodeServer?.netcodeServer.GetNetworkVariablesAsUpdates() }
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
                Console.WriteLine("GRPC_Ping > Connection lost with client.");
                DisconnectClient(context.Peer);
            }
        }

        #endregion

        #region Clients Connection

        public override async Task GRPC_SrvClientUpdate(GRPC_EmptyMsg request,
            IServerStreamWriter<GRPC_ClientUpdate> responseStream, ServerCallContext context)
        {
            if (!netcodeServer.HasValue)
            {
                _logger.LogError(
                    $"Presumed NetcodeServer {context.Peer} is trying to get ClientUpdate stream but NetcodeServer is not registered.");
                return;
            }
            if (netcodeServer.Value.adress != context.Peer)
            {
                _logger.LogError(
                    $"Client {context.Peer} is trying to get ClientUpdate stream but is not NetcodeServer {netcodeServer.Value.adress}.");
                return;
            }

            OnUnrealClientConnected += SendClientConnectedUpdate;
            OnUnrealClientDisconnected += SendClientDisconnectedUpdate;

            netcodeServer.Value.netcodeServer.ClientUpdateStream = responseStream;
            
            try
            {
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("GRPC_SrvClientUpdate > Connection lost with NetcodeServer.");
                UnsubscribeClientUpdateEvent();
                DisconnectClient(context.Peer);
            }
        }

        private async void SendClientConnectedUpdate(UnrealClient cli)
        {
            //This should never happen
            if (!netcodeServer.HasValue)
            {
                _logger.LogCritical("Trying to send client connected update without NetcodeServer connected.");
                UnsubscribeClientUpdateEvent();
                return;
            }
                        
            NetcodeServer srv = netcodeServer.Value.netcodeServer;
            
            try
            {
                await srv.ClientUpdateStream.WriteAsync(ToClientUpdate(cli, GRPC_ClientUpdateType.Connect));
            }
            catch (IOException)
            {
                Console.WriteLine("SendClientConnectedUpdate > Connection lost with NetcodeServer.");
                UnsubscribeClientUpdateEvent();
                DisconnectClient(srv.Adress);
            }
        }
        
        private async void SendClientDisconnectedUpdate(UnrealClient cli)
        {
            //This should never happen
            if (!netcodeServer.HasValue)
            {
                _logger.LogCritical("Trying to send client disconnected update without NetcodeServer connected.");
                UnsubscribeClientUpdateEvent();
                return;
            }
            
            NetcodeServer srv = netcodeServer.Value.netcodeServer;
            
            try
            {
                await srv.ClientUpdateStream.WriteAsync(ToClientUpdate(cli, GRPC_ClientUpdateType.Disconnect));
            }
            catch (IOException e)
            {
                Console.WriteLine("SendClientDisconnectedUpdate > Connection lost with NetcodeServer.");
                UnsubscribeClientUpdateEvent();
                DisconnectClient(srv.Adress);
            }
        }

        private GRPC_ClientUpdate ToClientUpdate(UnrealClient cli, GRPC_ClientUpdateType type) =>
            new() { ClientIP = cli.Adress, Type = type };

        private void UnsubscribeClientUpdateEvent()
        {
            OnUnrealClientConnected -= SendClientConnectedUpdate;
            OnUnrealClientDisconnected -= SendClientDisconnectedUpdate;
        }
        
        #endregion
        
        #region NetObjects / NetVars Update

        public override async Task<GRPC_EmptyMsg> GRPC_SrvNetObjUpdate(IAsyncStreamReader<GRPC_NetObjUpdate> requestStream, ServerCallContext context)
        {
            if (!netcodeServer.HasValue)
            {
                _logger.LogCritical("Trying to open NetObjUpdate stream without " +
                                    "NetcodeServer connected. Client IP: " + context.Peer);
                return new GRPC_EmptyMsg();
            }
            
            Console.WriteLine("NetcodeServer Network Objects update stream opened.");
            
            try
            {
                while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Got new NetworkObject update: type " + requestStream.Current.Type + ", netId " +
                                      requestStream.Current.NetId + ", prefabId " + requestStream.Current.PrefabId +
                                      "\n");
                    
                    netcodeServer.Value.netcodeServer.HandleNetObjUpdate(requestStream.Current);
                    
                    foreach (var client in unrealClients)
                    {
                        //If client has just connected and doesn't have a stream yet,
                        //queue the update for when it will have a stream
                        var stream = client.Value.NetObjectsStream;
                        
                        if (stream != null!)
                        {
                            await stream.WriteAsync(requestStream.Current);
                        }
                        else
                        {
                            client.Value.QueueNetObjUpdate(requestStream.Current);
                        }
                    }
                    
                    Console.WriteLine("Update sent to all unreal clients.\n");
                }
            }
            catch (IOException)
            {
                Console.WriteLine("GRPC_SrvNetObjUpdate > Connection lost with client.");
                DisconnectClient(context.Peer);
            }

            return new GRPC_EmptyMsg();
        }

        public override async Task GRPC_CliNetObjUpdate(GRPC_EmptyMsg request, IServerStreamWriter<GRPC_NetObjUpdate> responseStream, ServerCallContext context)
        {
            if (!unrealClients.ContainsKey(context.Peer))
            {
                _logger.LogError(
                    $"Client {context.Peer} is trying to get NetworkObjects update stream without being registered.");
                return;
            }

            Console.WriteLine($"Client NetObject stream opened");
            unrealClients[context.Peer].NetObjectsStream = responseStream;

            try
            {
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("GRPC_CliNetObjUpdate > Connection lost with client.");
                DisconnectClient(context.Peer);
            }
            Console.WriteLine($"Client NetObject stream closed");
        }
        
        public override async Task<GRPC_EmptyMsg> GRPC_SrvNetVarUpdate(IAsyncStreamReader<GRPC_NetVarUpdate> requestStream, ServerCallContext context)
        {
            Console.WriteLine($"NetVar writting stream opened");
            try
            {
                while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"NetVar received for HashName : {requestStream.Current.HashName} / Type {requestStream.Current.NewValue.Type} / New Value : {requestStream.Current.NewValue.Value}");
                    foreach (KeyValuePair<string, UnrealClient> unrealClient in unrealClients)
                    {
                        if (unrealClient.Value.netVarStream.ContainsKey(requestStream.Current.NewValue.Type) == false) continue;

                        Console.WriteLine($"JE TECRIS :  NetVar received for HashName : {requestStream.Current.HashName} / New Value : {requestStream.Current.NewValue.Value}");

                        if (netcodeServer.Value.netcodeServer.NetObjs[requestStream.Current.NetId].NetVars.ContainsKey(requestStream.Current.HashName))
                        {
                            netcodeServer.Value.netcodeServer.NetObjs[requestStream.Current.NetId].NetVars[requestStream.Current.HashName] = requestStream.Current.NewValue;
                        }
                        else
                        {
                            netcodeServer.Value.netcodeServer.NetObjs[requestStream.Current.NetId].NetVars.Add(requestStream.Current.HashName, requestStream.Current.NewValue);
                        }
                        
                        await unrealClient.Value.netVarStream[requestStream.Current.NewValue.Type].WriteAsync(requestStream.Current);
                    }

                    Console.WriteLine($"VRAIMENT TU AS RECU :  NetVar received for HashName : {requestStream.Current.HashName} / New Value : {requestStream.Current.NewValue.Value}");
                }
            }
            catch (IOException)
            {
                _logger.LogCritical("GRPC_SrvNetVarUpdate > Connection lost with client.");
                Console.WriteLine($"GRPC_SrvNetVarUpdate > stream closed");
                return new GRPC_EmptyMsg();
            }

            Console.WriteLine($"Wittring stream closed");
            return new GRPC_EmptyMsg();
        }

        public override async Task GRPC_CliNetNetVarUpdate(GRPC_GenericValue request, IServerStreamWriter<GRPC_NetVarUpdate> responseStream, ServerCallContext context)
        {
            Console.WriteLine($"Response stream opened : {context.Peer} / {request.Type}");

            if (unrealClients[context.Peer].netVarStream.TryAdd(request.Type, responseStream) == false)
            {
                Console.WriteLine($"Unreal client {context.Peer} already open listening stream for {request.Type}");
            }

            Console.WriteLine($"Check : {unrealClients[context.Peer].netVarStream[request.Type]}");
            try
            {
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                unrealClients[context.Peer].netVarStream.Remove(request.Type);
            }
            Console.WriteLine($"Response stream closed : {context.Peer} / {request.Type}");
        }

        #endregion
    }
}