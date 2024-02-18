using System;
using System.IO;
using System.Threading;
using Grpc.Core;
using GRPCClient;
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

        public bool HasPC => pcPlayerOwnerClientId != int.MaxValue;
        public bool HasMobile => mobilePlayerOwnerClientId != int.MaxValue;
        
        public UserInstance GetUserInstance(PlayerPlatform platform)
        {
            return UserInstanceManager.instance
                ? null
                : UserInstanceManager.instance.GetUserInstance(platform == PlayerPlatform.Pc
                    ? pcPlayerOwnerClientId
                    : mobilePlayerOwnerClientId);
        }

        public bool TryGetUserInstance(PlayerPlatform platform, out UserInstance user)
        {
            user = null;

            if (!UserInstanceManager.instance) return false;

            int clientId;
            
            switch (platform)
            {
                case PlayerPlatform.Pc:
                    if (pcPlayerOwnerClientId == int.MaxValue) return false;
                    clientId = pcPlayerOwnerClientId;
                    break;
                case PlayerPlatform.Mobile:
                    if (mobilePlayerOwnerClientId == int.MaxValue) return false;
                    clientId = mobilePlayerOwnerClientId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }

            return UserInstanceManager.instance.TryGetUserInstance(clientId, out user);
        }
    }
    
    public class TeamManager : NetworkSingleton<TeamManager>
    {
        private MainService.MainServiceClient _client => GRPC_NetworkManager.instance.client;
        
        public const uint MAX_TEAM = 3;
        public const uint MAX_PLAYER = MAX_TEAM * 2;
        // I cant ShowInInspector this array. It weirdly override my value set by the script
        private readonly TeamData[] _teams = new TeamData[MAX_TEAM];

        private const string _DEFAULT_PC_SLOT = "Click to join";
        private const string _DEFAULT_MOBILE_SLOT = "Empty";

        private AsyncDuplexStreamingCall<GRPC_TeamResponse, GRPC_Team> _teamManagerStream;
        private CancellationTokenSource _cancellationTokenSource;

        public readonly Event<int, string, PlayerPlatform> onTeamSetEvent = new Event<int, string, PlayerPlatform>(nameof(onTeamSetEvent));
        public readonly Event<int, string, PlayerPlatform> onPlayerReadyEvent = new Event<int, string, PlayerPlatform>(nameof(onPlayerReadyEvent));
        public readonly Event onAllPlayersReadyEvent = new Event(nameof(onAllPlayersReadyEvent));

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
/*#if UNITY_EDITOR
            // Unreal side
            FU_GRPC_NetworkManager.instance.onClientStartedEvent.Subscribe(this, FU_InitGrpcStream);
            FU_GRPC_NetworkManager.instance.onClientStopEvent.Subscribe(this, FU_DisposeGrpcStream);
#endif*/
            
            InitializeTeamsData();
            
            if (!IsServer && !IsHost) return;
            
            GRPC_NetworkManager.instance.onClientStartedEvent.Subscribe(this, InitGrpcStream);
            GRPC_NetworkManager.instance.onClientStopEvent.Subscribe(this, DisposeGrpcStream);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
/*#if UNITY_EDITOR
            // Unreal side
            FU_GRPC_NetworkManager.instance.onClientStartedEvent.Unsubscribe(FU_InitGrpcStream);
            FU_GRPC_NetworkManager.instance.onClientStopEvent.Unsubscribe(FU_DisposeGrpcStream);
#endif*/
            
            if (!IsServer && !IsHost) return;

            if (GRPC_NetworkManager.IsInstanceAlive())
            {
                GRPC_NetworkManager.instance.onClientStartedEvent.Unsubscribe(InitGrpcStream);
                GRPC_NetworkManager.instance.onClientStopEvent.Unsubscribe(DisposeGrpcStream);
            }
        }

        
        private void InitGrpcStream()
        {
            _teamManagerStream = _client.GRPC_TeamSelectionGrpcToNetcode();
            _cancellationTokenSource = new CancellationTokenSource();
            Read();
        }
        
        private void DisposeGrpcStream()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            
            _teamManagerStream.Dispose();
            _teamManagerStream = null;
        }
        
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

            if (UserInstanceManager.instance.GetUserInstance(ownerClientId).IsReady)
            {
                Debug.Log("Trying to join a team while being ready");
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
                OnTeamSet(previousUserTeamIndex, playerPlatform == PlayerPlatform.Pc ? _DEFAULT_PC_SLOT : _DEFAULT_MOBILE_SLOT, playerPlatform);
            }
            
            RegisterToTeamSlotLocal(ownerClientId, teamIndex, playerPlatform);
            userInstance.SetTeam(teamIndex);
            
            OnTeamSet(teamIndex, userInstance.PlayerName, playerPlatform);
            
            Debug.Log("Team recap :\n" +
                      $"Index : {teamIndex}\n" +
                      $"Pc : {_teams[teamIndex].pcPlayerOwnerClientId}\n" +
                      $"Mobile : {_teams[teamIndex].mobilePlayerOwnerClientId}");
        }

        private void RegisterToTeamSlotLocal(int ownerClientId, int teamIndex, PlayerPlatform playerPlatform)
        {
            TeamData teamData = _teams[teamIndex];
            if (playerPlatform == PlayerPlatform.Pc) teamData.pcPlayerOwnerClientId = ownerClientId;
            else teamData.mobilePlayerOwnerClientId = ownerClientId;
            _teams[teamIndex] = teamData;
        }
        
        /// <summary>
        /// Called by UserInstance._networkTeam callback on clients to populate team array
        /// </summary>
        public void ClientOnTeamChanged(UserInstance user, int oldTeam, int newTeam)
        {
            var platform = user.IsMobile ? PlayerPlatform.Mobile : PlayerPlatform.Pc;
            
            if (IsTeamIndexValid(oldTeam))
            {
                ResetTeamSlot(oldTeam, platform);
            }
            
            RegisterToTeamSlotLocal(user.ClientId, newTeam, platform);
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
        
        private void OnTeamSet(int teamIndex, string playerName, PlayerPlatform playerPlatform)
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

        public TeamData[] GetTeamsData()
        {
            // return _teams.Where((t, i) => IsTeamPlayerSlotAvailable(i, PlayerPlatform.Pc) == false).ToArray();
            return _teams;
        }
        
        public TeamData GetTeamData(int teamIndex)
        {
            return _teams[teamIndex];
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
    }
}
