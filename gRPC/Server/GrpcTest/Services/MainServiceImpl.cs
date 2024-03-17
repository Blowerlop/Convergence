using System.Drawing;
using Grpc.Core;
using Networking;

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

        public static readonly Dictionary<string, GRPCClient> clients = new();
        public static readonly Dictionary<string, UnrealClient> unrealClients = new Dictionary<string, UnrealClient>();
        public static NetcodeServer? netcodeServer = null;

        public static event Action<UnrealClient>? OnUnrealClientConnected;
        public static event Action<UnrealClient>? OnUnrealClientDisconnected;
        
        private void AddNetcodeServer(string ip)
        {
            if (netcodeServer != null)
            {
                Debug.LogWarning("AddNetcodeServer > Netcode server already connected. IP: " + ip);
                return;
            }
            if (clients.ContainsKey(ip))
            {
                Debug.LogWarning("AddNetcodeServer > Trying to connect a client that is already connected but not as Netcode server. IP: " + ip);
                return;
            }

            netcodeServer = new NetcodeServer(ip);
            clients.Add(ip, netcodeServer);
        }

        private void AddUnrealClient(string ip, string name)
        {
            if (unrealClients.ContainsKey(ip))
            {
                Debug.LogWarning("AddUnrealClient > Unreal client already connected. IP: " + ip);
                return;
            }
            if (clients.ContainsKey(ip))
            {
                Debug.LogWarning("AddUnrealClient > Trying to connect a client that is already connected but not as unreal client. IP: " + ip);
                return;
            }

            UnrealClient unrealClient = new UnrealClient(ip, name);
            unrealClient.id = -(clients.Keys.Count + 1);
            unrealClients.Add(ip, unrealClient);
            clients.Add(ip, unrealClient);

            OnUnrealClientConnected?.Invoke(unrealClient);
        }

        private void DisplayClients()
        {
            Debug.Log("-----------------------------------", Debug.SEPARATOR_COLOR);
            Console.WriteLine();

            Debug.Log($"Connected clients : {clients.Keys.Count}");
            Console.WriteLine();

            // Display Unreal clients
            Console.WriteLine($"Unreal clients : {unrealClients.Keys.Count}");
            foreach (KeyValuePair<string, UnrealClient> unrealClient in unrealClients)
            {
                Debug.Log(unrealClient.Value.Adress.ToString());
            }

            Console.WriteLine();
            Console.WriteLine();

            // Display NetcodeServer
            Debug.Log($"NetcodeServer :");
            if (netcodeServer != null)
            {
                Debug.Log(netcodeServer.Adress);
            }
            else
            {
                Debug.Log("Not connected");
            }
            Console.WriteLine();

            Debug.Log("-----------------------------------", Debug.SEPARATOR_COLOR);
            Console.WriteLine();
        }

        public void DisconnectClient(string clientAdress)
        {
            if(!clients.TryGetValue(clientAdress, out var client)) 
            {
                //Can't use logger since this is static
                Debug.LogWarning("DisconnectClient > Trying to disconnect a client that is not connected. IP: " + clientAdress);
                return; 
            }
            
            client.Disconnect();
            
            Debug.Log($"DisconnectClient > Disconnect {clientAdress}\n");

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
            Debug.Log("Handshake", ConsoleColor.Magenta);
            Debug.Log(context.Host, ConsoleColor.Magenta);
            Debug.Log(context.Peer + "\n", ConsoleColor.Magenta);

            if (clients.Count > 0)
            {
                // Catch if client is already added to the clients
                string clientAdress = context.Peer;
                AddUnrealClient(clientAdress, request.Name);
                

                DisplayClients();

                if (netcodeServer == null)
                {
                    Debug.LogError("GRPC_Handshake > NetcodeServer is null");
                    return Task.FromResult(new GRPC_HandshakeGet { Result = 1, ClientId = -1 });
                }

                return Task.FromResult(new GRPC_HandshakeGet
                {
                    Result = 0, //0 = good!
                    ClientId = unrealClients[clientAdress].id,
                    
                    NetObjects = { netcodeServer.GetNetworkObjectsAsUpdates() },
                    NetVars = { netcodeServer.GetNetworkVariablesAsUpdates() }
                });
            }
            else
            {
                Debug.LogError("GRPC_Handshake > Getting Handshake, but there is no NetcodeServer. " +
                    "Connect UnrealClients after NetcodeServer!");

                //Result != 0 => error
                return Task.FromResult(new GRPC_HandshakeGet { Result = 1, ClientId = -1 });
            }
        }

        public override Task<GRPC_NHandshakeGet> GRPC_NetcodeHandshake(GRPC_NHandshakePost request, ServerCallContext context)
        {
            //Debug
            Debug.Log("NetcodeHandshake", ConsoleColor.Magenta);
            Debug.Log(context.Host, ConsoleColor.Magenta);
            Debug.Log(context.Peer + "\n", ConsoleColor.Magenta);

            if (netcodeServer == null)
            {
                string adress = context.Peer;
                AddNetcodeServer(adress);

                DisplayClients();

                return Task.FromResult(new GRPC_NHandshakeGet { Result = 0 });
            }
            
            Debug.LogError("GRPC_NetcodeHandshake > Getting NetcodeHandshake, but there is already an active NetcodeServer!");

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
                Debug.Log("GRPC_Ping > Connection lost with client.");
                DisconnectClient(context.Peer);
            }
        }

        #endregion

        #region Clients Connection

        public override async Task GRPC_SrvClientUpdate(GRPC_EmptyMsg request,
            IServerStreamWriter<GRPC_ClientUpdate> responseStream, ServerCallContext context)
        {
            if (netcodeServer == null)
            {
                Debug.LogError(
                    $"GRPC_SrvClientUpdate > Presumed NetcodeServer {context.Peer} is trying to get ClientUpdate stream but NetcodeServer is not registered.");
                return;
            }
            if (netcodeServer.Adress != context.Peer)
            {
                Debug.LogError(
                    $"GRPC_SrvClientUpdate > Client {context.Peer} is trying to get ClientUpdate stream but is not NetcodeServer {netcodeServer.Adress}.");
                return;
            }

            OnUnrealClientConnected += SendClientConnectedUpdate;
            OnUnrealClientDisconnected += SendClientDisconnectedUpdate;

            netcodeServer.ClientUpdateStream = responseStream;
            
            try
            {
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                Debug.Log("GRPC_SrvClientUpdate > Connection lost with NetcodeServer.");
                UnsubscribeClientUpdateEvent();
                DisconnectClient(context.Peer);
            }
        }

        private async void SendClientConnectedUpdate(UnrealClient cli)
        {
            //This should never happen
            if (netcodeServer == null)
            {
                Debug.LogError("SendClientConnectedUpdate > Trying to send client connected update without NetcodeServer connected.");
                UnsubscribeClientUpdateEvent();
                return;
            }
                        
            try
            {
                await netcodeServer.ClientUpdateStream.WriteAsync(ToClientUpdate(cli, GRPC_ClientUpdateType.Connect));
            }
            catch (IOException)
            {
                Debug.Log("SendClientConnectedUpdate > SendClientConnectedUpdate > Connection lost with NetcodeServer.");
                UnsubscribeClientUpdateEvent();
                DisconnectClient(netcodeServer.Adress);
            }
        }
        
        private async void SendClientDisconnectedUpdate(UnrealClient cli)
        {
            //This should never happen
            if (netcodeServer == null)
            {
                Debug.LogError("SendClientDisconnectedUpdate > Trying to send client disconnected update without NetcodeServer connected.");
                UnsubscribeClientUpdateEvent();
                return;
            }
            
            try
            {
                await netcodeServer.ClientUpdateStream.WriteAsync(ToClientUpdate(cli, GRPC_ClientUpdateType.Disconnect));
            }
            catch (IOException)
            {
                Debug.Log("SendClientDisconnectedUpdate > Connection lost with NetcodeServer.");
                UnsubscribeClientUpdateEvent();
                DisconnectClient(netcodeServer.Adress);
            }
        }

        private GRPC_ClientUpdate ToClientUpdate(UnrealClient cli, GRPC_ClientUpdateType type) =>
            new() { ClientIP = cli.Adress, Type = type, ClientId = cli.id, Name = cli.name};

        private void UnsubscribeClientUpdateEvent()
        {
            OnUnrealClientConnected -= SendClientConnectedUpdate;
            OnUnrealClientDisconnected -= SendClientDisconnectedUpdate;
        }
        
        #endregion
        
        #region NetObjects / NetVars Update
        
        public override async Task<GRPC_EmptyMsg> GRPC_SrvNetObjUpdate(IAsyncStreamReader<GRPC_NetObjUpdate> requestStream, IServerStreamWriter<GRPC_EmptyMsg> responseStream,
            ServerCallContext context)
        {
            if (netcodeServer == null)
            {
                Debug.LogError("GRPC_SrvNetObjUpdate > Trying to open NetObjUpdate stream without " +
                                    "NetcodeServer connected. Client IP: " + context.Peer);
                return new GRPC_EmptyMsg();
            }
            
            Debug.Log("GRPC_SrvNetObjUpdate > NetcodeServer Network Objects update stream opened.");
            
            try
            {
                while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
                {
                    
                    Debug.Log("GRPC_SrvNetObjUpdate > Got new NetworkObject update: type " + requestStream.Current.Type + ", netId " +
                                      requestStream.Current.NetId + ", prefabId " + requestStream.Current.PrefabId +
                                      "\n");

                    lock (NetworkObject.Locker)
                    {
                        netcodeServer.HandleNetObjUpdate(requestStream.Current);
                    }
                    
                    await responseStream.WriteAsync(new GRPC_EmptyMsg());
                    
                    await netcodeServer.NetObjectsStream.WriteAsync(requestStream.Current);
                    
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

                    

                    Debug.Log("GRPC_SrvNetObjUpdate > Update sent to all unreal clients.\n");
                }
            }
            catch (IOException)
            {
                Debug.Log("GRPC_SrvNetObjUpdate > Connection lost with client - NetObj Stream closed");
                //DisconnectClient(context.Peer);
            }

            return new GRPC_EmptyMsg();
        }

        public override async Task GRPC_CliNetObjUpdate(GRPC_EmptyMsg request, IServerStreamWriter<GRPC_NetObjUpdate> responseStream, ServerCallContext context)
        {
            if (netcodeServer.Adress == context.Peer)
            {
                netcodeServer.NetObjectsStream = responseStream;
            }
            else
            {
                if (!unrealClients.ContainsKey(context.Peer))
                {
                    Debug.LogError(
                        $"GRPC_CliNetObjUpdate > Client {context.Peer} is trying to get NetworkObjects update stream without being registered.");
                    return;
                }

                Debug.Log($"GRPC_CliNetObjUpdate > Client NetObject stream opened");
                unrealClients[context.Peer].NetObjectsStream = responseStream;
            }
            
            try
            {
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                Debug.Log("GRPC_CliNetObjUpdate > Connection lost with client - NetObj Client NetObject stream closed");
                //DisconnectClient(context.Peer);
            }

            Debug.Log($"GRPC_CliNetObjUpdate > NetObj Client NetObject stream closed");
        }

        public override async Task<GRPC_EmptyMsg> GRPC_SrvNetVarUpdate(IAsyncStreamReader<GRPC_NetVarUpdate> requestStream, ServerCallContext context)
        {
            Debug.Log($"GRPC_SrvNetVarUpdate > NetVar writing stream opened");
            
            try
            {
                while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
                {
                    if (netcodeServer.NetObjs.ContainsKey(requestStream.Current.NetId) == false)
                    {
                        Debug.LogError("Net id not present : " + requestStream.Current.NetId);
                        continue;
                    }

                    Debug.Log($"GRPC_SrvNetVarUpdate > NetVar received for HashName : {requestStream.Current.HashName} / Type {requestStream.Current.NewValue.Type} / New Value : {requestStream.Current.NewValue.Value} / Net id : {requestStream.Current.NetId}");

                    netcodeServer.NetObjs[requestStream.Current.NetId].NetVars[requestStream.Current.HashName] = requestStream.Current.NewValue;
                    
                    foreach (KeyValuePair<string, UnrealClient> unrealClient in unrealClients)
                    {
                        //There could be a problem if a client does GRPC_CliNetNetVarUpdate
                        //and at the same time netcode server send a net var update
                        if (unrealClient.Value.netVarStream.ContainsKey(requestStream.Current.NewValue.Type) == false) continue;

                        Debug.Log($"GRPC_SrvNetVarUpdate > Unreal client receiving NetVar : HashName : {requestStream.Current.HashName} / New Value : {requestStream.Current.NewValue.Value}");

                        await unrealClient.Value.netVarStream[requestStream.Current.NewValue.Type].WriteAsync(requestStream.Current);
                    }

                    //Console.WriteLine($"GRPC_SrvNetVarUpdate > VRAIMENT TU AS RECU :  NetVar received for HashName : {requestStream.Current.HashName} / New Value : {requestStream.Current.NewValue.Value}");
                }
            }
            catch (IOException)
            {
                //Debug.Log("GRPC_SrvNetVarUpdate > Connection lost with client.");
                Debug.Log($"GRPC_SrvNetVarUpdate > Connection lost with client - NetVar Writing stream closed");
                if (netcodeServer != null)
                {
                    foreach (var netObjs in netcodeServer.NetObjs.Values)
                    {
                        netObjs.NetVars.Clear();
                    }

                    netcodeServer.NetObjs.Clear();
                }
                
                return new GRPC_EmptyMsg();
            }


            if (netcodeServer != null)
            {
                foreach (var netObjs in netcodeServer.NetObjs.Values)
                {
                    netObjs.NetVars.Clear();
                }

                netcodeServer.NetObjs.Clear();
            }

            Debug.Log($"GRPC_SrvNetVarUpdate > Witring stream closed manually");
            return new GRPC_EmptyMsg();
        }

        public override async Task GRPC_CliNetNetVarUpdate(GRPC_GenericValue request, IServerStreamWriter<GRPC_NetVarUpdate> responseStream, ServerCallContext context)
        {
            Debug.Log($"Response stream opened : {context.Peer} / {request.Type}");
            
            lock (unrealClients[context.Peer].netVarStream)
            {
                if (unrealClients[context.Peer].netVarStream.TryAdd(request.Type, responseStream) == false)
                {
                    Debug.Log($"GRPC_CliNetNetVarUpdate > Unreal client {context.Peer} already open listening stream for {request.Type}");
                }

                Debug.Log($"GRPC_CliNetNetVarUpdate > Check : {unrealClients[context.Peer].netVarStream[request.Type]}");
            }
            
            foreach (GRPC_NetVarUpdate netVarUpdate in netcodeServer.GetNetworkVariablesAsUpdates()
                         .Where(netVarUpdate => netVarUpdate.NewValue.Type == request.Type))
            {
                await responseStream.WriteAsync(netVarUpdate);
            }

            try 
            {
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                if(unrealClients.TryGetValue(context.Peer, out var client))
                    client.netVarStream.Remove(request.Type);
            }
            Debug.Log($"GRPC_CliNetNetVarUpdate > Response stream closed : {context.Peer} / {request.Type}");
        }
        #endregion

        #region Lobby
        public override async Task GRPC_TeamSelectionUnrealToGrpc(IAsyncStreamReader<GRPC_Team> requestStream, IServerStreamWriter<GRPC_TeamResponse> responseStream, ServerCallContext context)
        {
            if (netcodeServer == null)
            {
                Debug.LogError("GRPC_TeamSelectionUnrealToGrpc > NetcodeServer is null");
                return;
            }

            netcodeServer.teamSelectionResponseStream.Add(responseStream);

            Debug.Log("GRPC_TeamSelectionUnrealToGrpc > Opened stream");
            try
            {
                while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
                {
                    GRPC_Team messageReceived = requestStream.Current;
                    await UnrealClient.teamSelectionResponseStream.WriteAsync(messageReceived);

                }
            }
            catch (IOException)
            {
                Debug.Log("GRPC_TeamSelectionUnrealToGrpc > Connection lost with client - TeamSelection stream closed");
                //DisconnectClient(context.Peer);
            }
        }

        public override async Task GRPC_TeamSelectionGrpcToNetcode(IAsyncStreamReader<GRPC_TeamResponse> requestStream, IServerStreamWriter<GRPC_Team> responseStream, ServerCallContext context)
        {
            Debug.Log("GRPC_TeamSelectionGrpcToNetcode > Opened stream");
            UnrealClient.teamSelectionResponseStream = responseStream;

            try
            {
                while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
                {
                    // Callback the response to all the Unreal clients
                    foreach (IServerStreamWriter<GRPC_TeamResponse> item in netcodeServer.teamSelectionResponseStream)
                    {
                        await item.WriteAsync(requestStream.Current);
                    }
                    
                }
            }
            catch (IOException)
            {
                Debug.Log("GRPC_TeamSelectionGrpcToNetcode > Connection lost with client - TeamSelection stream closed");
                //DisconnectClient(context.Peer);
            }
        }
        #endregion
        
        #region Spells
        
        // Forward Unreal spell cast request to Netcode server
        public override Task<GRPC_EmptyMsg> GRPC_SetUnrealSpellUnrealToGrpc(GRPC_SpellSlot request, ServerCallContext context)
        {
            GRPC_EmptyMsg empty = new();
            
            if (netcodeServer == null)
            {
                Debug.LogError("GRPC_SetUnrealSpellUnrealToGrpc > Can't forward spell cast request to NetcodeServer because it is null!");
                return Task.FromResult(empty);
            }
            if(netcodeServer.SetUnrealSpellStream == null)
            {
                Debug.LogError("GRPC_SetUnrealSpellUnrealToGrpc > Can't forward spell cast request to " +
                               "NetcodeServer because SetUnrealSpellStream is null!");
                return Task.FromResult(empty);
            }

            var client = unrealClients.Values.FirstOrDefault(cli => cli.Adress == context.Peer);

            if (client == null)
            {
                Debug.LogError($"GRPC_SetUnrealSpellUnrealToGrpc > Received a spell cast request from " +
                               $"'{context.Peer}' but no UnrealClient is registered with this IP!");
                return Task.FromResult(empty);
            }

            request.ClientId = client.id;
            netcodeServer.SetUnrealSpellStream.WriteAsync(request);
            
            return Task.FromResult(empty);
        }
        
        // Netcode server uses this to open a stream to receive spell cast requests from Unreal clients
        public override async Task GRPC_SetUnrealSpellGrpcToNetcode(GRPC_EmptyMsg request,
            IServerStreamWriter<GRPC_SpellSlot> responseStream, ServerCallContext context)
        {
            if (netcodeServer == null)
            {
                Debug.LogError(
                    $"GRPC_SetUnrealSpellGrpcToNetcode > Presumed NetcodeServer {context.Peer} is trying to " +
                    $"get SetUnrealSpellStream but NetcodeServer is not registered.");
                return;
            }
            if (netcodeServer.Adress != context.Peer)
            {
                Debug.LogError(
                    $"GRPC_SetUnrealSpellGrpcToNetcode > Client {context.Peer} is trying to get SetUnrealSpellStream " +
                    $"but is not NetcodeServer {netcodeServer.Adress}.");
                return;
            }
            
            netcodeServer.SetUnrealSpellStream = responseStream;
            
            try
            {
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                Debug.Log("GRPC_SetUnrealSpellGrpcToNetcode > Connection lost with NetcodeServer.");
                UnsubscribeClientUpdateEvent();
                DisconnectClient(context.Peer);
            }
        }
        
        // Forward Unreal spell cast request to Netcode server
        public override Task<GRPC_EmptyMsg> GRPC_SpellCastRequestUnrealToGrpc(GRPC_SpellCastRequest request, ServerCallContext context)
        {
            GRPC_EmptyMsg empty = new();
            
            if (netcodeServer == null)
            {
                Debug.LogError("GRPC_SpellCastRequestUnrealToGrpc > Can't forward spell cast request to NetcodeServer because it is null!");
                return Task.FromResult(empty);
            }
            if(netcodeServer.SpellCastRequestStream == null)
            {
                Debug.LogError("GRPC_SpellCastRequestUnrealToGrpc > Can't forward spell cast request to " +
                               "NetcodeServer because SpellCastRequestStream is null!");
                return Task.FromResult(empty);
            }

            var client = unrealClients.Values.FirstOrDefault(cli => cli.Adress == context.Peer);

            if (client == null)
            {
                Debug.LogError($"GRPC_SpellCastRequestUnrealToGrpc > Received a spell cast request from " +
                               $"'{context.Peer}' but no UnrealClient is registered with this IP!");
                return Task.FromResult(empty);
            }

            request.ClientId = client.id;
            netcodeServer.SpellCastRequestStream.WriteAsync(request);
            
            return Task.FromResult(empty);
        }
        
        // Netcode server uses this to open a stream to receive spell cast requests from Unreal clients
        public override async Task GRPC_SpellCastRequestGrpcToNetcode(GRPC_EmptyMsg request,
            IServerStreamWriter<GRPC_SpellCastRequest> responseStream, ServerCallContext context)
        {
            if (netcodeServer == null)
            {
                Debug.LogError(
                    $"GRPC_SpellCastRequestGrpcToNetcode > Presumed NetcodeServer {context.Peer} is trying to " +
                    $"get Spells stream but NetcodeServer is not registered.");
                return;
            }
            if (netcodeServer.Adress != context.Peer)
            {
                Debug.LogError(
                    $"GRPC_SpellCastRequestGrpcToNetcode > Client {context.Peer} is trying to get Spells stream " +
                    $"but is not NetcodeServer {netcodeServer.Adress}.");
                return;
            }
            
            netcodeServer.SpellCastRequestStream = responseStream;
            
            try
            {
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                Debug.Log("GRPC_SpellCastRequestGrpcToNetcode > Connection lost with NetcodeServer.");
                UnsubscribeClientUpdateEvent();
                DisconnectClient(context.Peer);
            }
        }
        
        #endregion
    }
}