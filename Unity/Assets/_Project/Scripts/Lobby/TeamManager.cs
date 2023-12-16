using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Grpc.Core;
using GRPCClient;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public enum PlayerPlatform
    {
        Pc,
        Mobile
    }
    
    [System.Serializable]
    public struct TeamData
    {
        public int pcPlayerOwnerClientId;
        public int mobilePlayerOwnerClientId;
    }
    
    public class TeamManager : NetworkSingleton<TeamManager>
    {
        private MainService.MainServiceClient _client => GRPC_NetworkManager.instance.client;
        
        public const uint MAX_TEAM = 3;
        public const uint MAX_PLAYER = MAX_TEAM * 2;
        // I cant ShowInInspector this array. It weirdly override my value set by the script
        private readonly TeamData[] _teams = new TeamData[MAX_TEAM];

        private const string DEFAULT_PC_SLOT = "Click to join";
        private const string DEFAULT_MOBILE_SLOT = "Empty";

        private AsyncDuplexStreamingCall<GRPC_TeamResponse, GRPC_Team> _teamManagerStream;
        private CancellationTokenSource _cancellationTokenSource;

        public readonly Event<int, string, PlayerPlatform> onTeamSetEvent = new Event<int, string, PlayerPlatform>(nameof(onTeamSetEvent));
        public readonly Event<int, string, PlayerPlatform> onPlayerReadyEvent = new Event<int, string, PlayerPlatform>(nameof(onPlayerReadyEvent));
        public readonly Event onAllPlayersReadyEvent = new Event(nameof(onAllPlayersReadyEvent));
        
        #if UNITY_EDITOR
        private AsyncDuplexStreamingCall<GRPC_Team, GRPC_TeamResponse> _FU_teamManagerStream;
        private CancellationTokenSource _FU_cancellationTokenSource;
        #endif
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
#if UNITY_EDITOR
            // Unreal side
            FU_GRPC_NetworkManager.instance.onClientStartedEvent.Subscribe(this, FU_InitGrpcStream);
            FU_GRPC_NetworkManager.instance.onClientStopEvent.Subscribe(this, FU_DisposeGrpcStream);
#endif
            
            if (!IsServer && !IsHost) return;
            
            InitializeTeamsData();
            
            GRPC_NetworkManager.instance.onClientStartedEvent.Subscribe(this, InitGrpcStream);
            GRPC_NetworkManager.instance.onClientStopEvent.Subscribe(this, DisposeGrpcStream);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
#if UNITY_EDITOR
            // Unreal side
            FU_GRPC_NetworkManager.instance.onClientStartedEvent.Unsubscribe(FU_InitGrpcStream);
            FU_GRPC_NetworkManager.instance.onClientStopEvent.Unsubscribe(FU_DisposeGrpcStream);
#endif
            
            if (!IsServer && !IsHost) return;
            
            GRPC_NetworkManager.instance.onClientStartedEvent.Unsubscribe(InitGrpcStream);
            GRPC_NetworkManager.instance.onClientStopEvent.Unsubscribe(DisposeGrpcStream);
        }

        
        private void InitGrpcStream()
        {
            _teamManagerStream = _client.GRPC_TeamSelectionGrpcToNetcode();
            _cancellationTokenSource = new CancellationTokenSource();
            Read();
        }
        
        #if UNITY_EDITOR
        private void FU_InitGrpcStream()
        {
            _FU_teamManagerStream = FU_GRPC_NetworkManager.instance.client.GRPC_TeamSelectionUnrealToGrpc();
            _FU_cancellationTokenSource = new CancellationTokenSource();
            FU_Read();
        }
        #endif

        private void DisposeGrpcStream()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            
            _teamManagerStream.Dispose();
            _teamManagerStream = null;
        }
        
        #if UNITY_EDITOR
        private void FU_DisposeGrpcStream()
        {
            _FU_cancellationTokenSource.Cancel();
            _FU_cancellationTokenSource.Dispose();
            _FU_cancellationTokenSource = null;
            
            _FU_teamManagerStream.Dispose();
            _FU_teamManagerStream = null;
        }
        #endif
        
        private void InitializeTeamsData()
        {
            Debug.Log("InitializeTeamsData");
            for (int i = 0; i < _teams.Length; i++)
            {
                TeamData teamData = new TeamData
                {
                    pcPlayerOwnerClientId = int.MaxValue,
                    mobilePlayerOwnerClientId = int.MaxValue
                };

                _teams[i] = teamData;
            }
        }

        public bool TryGetTeam(int teamIndex, out TeamData teamData)
        {
            if (IsTeamIndexValid(teamIndex) == false)
            {
                teamData = default;
                return false;
            }
            
            teamData = _teams[teamIndex];
            
            return true;
        }

        public bool TrySetTeam(int ownerClientId, int teamIndex, PlayerPlatform playerPlatform)
        {
            if (IsTeamIndexValid(teamIndex) == false)
            {
                Debug.LogError($"Try to set an invalid team : Invalid is {teamIndex}");
                return false;
            }

            if (IsTeamPlayerSlotAvailable(teamIndex, playerPlatform) == false)
            {
                Debug.Log("Trying to join a team slot that are already occupied");
                return false;
            }

            Debug.Log("Try set team ok");
            SetTeam(ownerClientId, teamIndex, playerPlatform);
            return true;;
        }
        
        private void SetTeam(int ownerClientId, int teamIndex, PlayerPlatform playerPlatform)
        {
            UserInstance userInstance = UserInstanceManager.instance.GetUserInstance(ownerClientId);
            
            int previousUserTeamIndex = userInstance.Team;
            // Reset previous team slot if valid
            if (IsTeamIndexValid(previousUserTeamIndex))
            {
                ResetTeamSlot(previousUserTeamIndex, playerPlatform);
                OnTeamSetServerRpc(previousUserTeamIndex, playerPlatform == PlayerPlatform.Pc ? DEFAULT_PC_SLOT : DEFAULT_MOBILE_SLOT, playerPlatform);
            }
            
            RegisterToTeamSlot(ownerClientId, teamIndex, playerPlatform);
            userInstance.SetTeamServerRpc(teamIndex);
            
            OnTeamSetServerRpc(teamIndex, userInstance.PlayerName, playerPlatform);
            
            Debug.Log("Team recap :\n" +
                      $"Index : {teamIndex}\n" +
                      $"Pc : {_teams[teamIndex].pcPlayerOwnerClientId}\n" +
                      $"Mobile : {_teams[teamIndex].mobilePlayerOwnerClientId}");
        }

        private void RegisterToTeamSlot(int ownerClientId, int teamIndex, PlayerPlatform playerPlatform)
        {
            TeamData teamData = _teams[teamIndex];
            if (playerPlatform == PlayerPlatform.Pc) teamData.pcPlayerOwnerClientId = ownerClientId;
            else teamData.mobilePlayerOwnerClientId = ownerClientId;
            _teams[teamIndex] = teamData;
        }

        private void ResetTeamSlot(int teamIndex, PlayerPlatform playerPlatform)
        {
            TeamData teamData = _teams[teamIndex];
            if (playerPlatform == PlayerPlatform.Pc) teamData.pcPlayerOwnerClientId = int.MaxValue;
            else teamData.mobilePlayerOwnerClientId = int.MaxValue;
            _teams[teamIndex] = teamData;
        }

        public bool IsTeamPlayerSlotAvailable(int teamIndex, PlayerPlatform playerPlatform)
        {
            if (TryGetTeam(teamIndex, out TeamData teamData))
            {
                if (playerPlatform == PlayerPlatform.Pc)
                {
                    return teamData.pcPlayerOwnerClientId == int.MaxValue;
                }

                return teamData.mobilePlayerOwnerClientId == int.MaxValue;
            }

            return false;
        }

        public bool IsTeamIndexValid(int teamIndex)
        {
            return !(teamIndex < 0 || teamIndex >= _teams.Length);
        }
        
        [ServerRpc]
        private void OnTeamSetServerRpc(int teamIndex, string playerName, PlayerPlatform playerPlatform)
        {
            OnTeamSetLocal(teamIndex, playerName, playerPlatform);
            OnTeamSetClientRpc(teamIndex, playerName, playerPlatform);
        }
        
        [ClientRpc]
        private void OnTeamSetClientRpc(int teamIndex, string playerName, PlayerPlatform playerPlatform)
        {
            OnTeamSetLocal(teamIndex, playerName, playerPlatform);
        }

        private void OnTeamSetLocal(int teamIndex, string playerName, PlayerPlatform playerPlatform)
        {
            onTeamSetEvent.Invoke(this, true, teamIndex, playerName, playerPlatform);
        }

        public TeamData[] GetTeams()
        {
            // return _teams.Where((t, i) => IsTeamPlayerSlotAvailable(i, PlayerPlatform.Pc) == false).ToArray();
            return _teams;
        }

        public void PlayerReady()
        {
            PlayerReadyServerRpc((int)NetworkManager.Singleton.LocalClientId);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void PlayerReadyServerRpc(int clientId)
        {
            UserInstance userInstance = UserInstanceManager.instance.GetUserInstance(clientId);
            if (userInstance == null)
            {
                Debug.LogError("User instance is null");
                return;
            }

            if (userInstance.IsReady) return;
            
            userInstance.SetIsReadyServerRpc(true);

            UserInstance[] usersInstance = UserInstanceManager.instance.GetUsersInstance();
            int numberOfPlayersReady = usersInstance.Count(x => x.IsReady);
            if (numberOfPlayersReady == usersInstance.Length)
            {
                Debug.Log("All players ready");
                OnAllPlayersReadyServerRpc();
            }
        }

        [ServerRpc]
        private void OnAllPlayersReadyServerRpc()
        {
            OnAllPlayersReadyClientRpc();
        }
        
        [ClientRpc]
        private void OnAllPlayersReadyClientRpc()
        {
            OnAllPlayersReadyLocal();
        }
        
        private void OnAllPlayersReadyLocal()
        {
            onAllPlayersReadyEvent.Invoke(this, true);
        }
        

        private async void Write(GRPC_TeamResponse response)
        {
            try
            {
                await _teamManagerStream.RequestStream.WriteAsync(response, _cancellationTokenSource.Token);
            }
            catch (IOException)
            {
                if (GRPC_NetworkManager.instance.isConnected)
                    GRPC_NetworkManager.instance.StopClient();
            }
        }
        
        private async void Read()
        {
            Debug.Log("Read");
            try
            {
                while (await _teamManagerStream.ResponseStream.MoveNext(_cancellationTokenSource.Token))
                {
                    Debug.Log("Message received");
                    GRPC_Team messageReceived = _teamManagerStream.ResponseStream.Current;
                    bool response = TrySetTeam(messageReceived.ClientId, messageReceived.TeamIndex, PlayerPlatform.Mobile);
                    Write(new GRPC_TeamResponse {Team = messageReceived, Response = response});
                }
            }
            catch (RpcException)
            {
                if (GRPC_NetworkManager.instance.isConnected)
                {
                    GRPC_NetworkManager.instance.StopClient();
                }
            }
        }
        
        
        public async void FU_Write(ulong ownerClientId, int teamIndex)
        {
            try
            {
                Debug.Log("FU Write");
                await _FU_teamManagerStream.RequestStream.WriteAsync(new GRPC_Team{ClientId = (int)ownerClientId, TeamIndex = teamIndex}, _FU_cancellationTokenSource.Token);
            }
            catch (IOException)
            {
                if (FU_GRPC_NetworkManager.instance.isConnected)
                    FU_GRPC_NetworkManager.instance.StopClient();
            }
        }
        
        private async void FU_Read()
        {
            try
            {
                while (await _FU_teamManagerStream.ResponseStream.MoveNext(_FU_cancellationTokenSource.Token))
                {
                    GRPC_TeamResponse messageReceived = _FU_teamManagerStream.ResponseStream.Current;
                    Debug.Log("Response : " + messageReceived.Response);
                }
            }
            catch (RpcException)
            {
                if (FU_GRPC_NetworkManager.instance.isConnected)
                {
                    FU_GRPC_NetworkManager.instance.StopClient();
                }
            }
        }
    }
}
