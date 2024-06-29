using System;
using System.IO;
using System.Linq;
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

        public bool HasPC => pcPlayerOwnerClientId != TeamManager.UNASSIGNED_TEAM_INDEX;
        public bool HasMobile => mobilePlayerOwnerClientId != TeamManager.UNASSIGNED_TEAM_INDEX;
        
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
                    if (pcPlayerOwnerClientId == TeamManager.UNASSIGNED_TEAM_INDEX) return false;
                    clientId = pcPlayerOwnerClientId;
                    break;
                case PlayerPlatform.Mobile:
                    if (mobilePlayerOwnerClientId == TeamManager.UNASSIGNED_TEAM_INDEX) return false;
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
        public const int UNASSIGNED_TEAM_INDEX = -1;
        // I cant ShowInInspector this array. It weirdly override my value set by the script
        private readonly TeamData[] _teams = new TeamData[MAX_TEAM];

        public const string DEFAULT_PC_SLOT_TEXT = "CLICK TO JOIN";
        public const string DEFAULT_MOBILE_SLOT_TEXT = "<i>Mobile  -  EMPTY</i>"; 

        private AsyncDuplexStreamingCall<GRPC_TeamResponse, GRPC_Team> _teamManagerStream;
        private CancellationTokenSource _cancellationTokenSource;

        public Action<int, string, PlayerPlatform> onTeamSet;

        
        public override void OnNetworkSpawn()
        {
            InitializeTeamsData();
            if (IsServer) UserInstance.OnDespawned += OnUserInstanceDespawned_CleanTeam;
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer) UserInstance.OnDespawned -= OnUserInstanceDespawned_CleanTeam;
        }


        private void InitializeTeamsData()
        {
            Debug.Log("InitializeTeamsData");
            for (int i = 0; i < _teams.Length; i++)
            {
                TeamData teamData = new TeamData
                {
                    pcPlayerOwnerClientId = UNASSIGNED_TEAM_INDEX,
                    mobilePlayerOwnerClientId = UNASSIGNED_TEAM_INDEX
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
            
            if(teamIndex != UNASSIGNED_TEAM_INDEX && playerPlatform == PlayerPlatform.Mobile && GetTeamData(teamIndex).HasPC == false)
            {
                Debug.Log("Trying to join a mobile team without a pc player");
                return false;
            }

            if (UserInstanceManager.instance.GetUserInstance(ownerClientId).IsReady)
            {
                Debug.Log("Trying to join a team while being ready");
                return false;
            }

            Debug.Log("Try set team ok");
            SetTeam(ownerClientId, teamIndex, playerPlatform);
            return true;
        }
        
        private void SetTeam(int ownerClientId, int teamIndex, PlayerPlatform playerPlatform)
        {
            UserInstance userInstance = UserInstanceManager.instance.GetUserInstance(ownerClientId);
            
            int previousUserTeamIndex = userInstance.Team;
            // Reset previous team slot if valid
            if (IsTeamIndexValid(previousUserTeamIndex))
            {
                string playerName = string.Empty;
                
                if (previousUserTeamIndex == UNASSIGNED_TEAM_INDEX)
                {
                    bool shouldBeMobile = playerPlatform == PlayerPlatform.Mobile;
                    playerName = string.Join(" / ", UserInstanceManager.instance.All().Where(x => x.Team == UNASSIGNED_TEAM_INDEX && x.IsMobile == shouldBeMobile && x.ClientId != ownerClientId).Select(x => x.PlayerName));
                    
                    if (string.IsNullOrEmpty(playerName))
                        playerName = playerPlatform == PlayerPlatform.Pc ? DEFAULT_PC_SLOT_TEXT : DEFAULT_MOBILE_SLOT_TEXT;
                }
                else
                {
                    ResetTeamSlot(previousUserTeamIndex, playerPlatform);
                }
                
                // Always empty if previous team was unassigned
                if (string.IsNullOrEmpty(playerName))
                    playerName = playerPlatform == PlayerPlatform.Pc ? DEFAULT_PC_SLOT_TEXT : DEFAULT_MOBILE_SLOT_TEXT;
                
                
                OnTeamSet(previousUserTeamIndex, playerName, playerPlatform);
            }
            
            if (teamIndex != UNASSIGNED_TEAM_INDEX) RegisterToTeamSlotLocal(ownerClientId, teamIndex, playerPlatform);
            userInstance.SrvSetTeam(teamIndex);

            if (teamIndex == UNASSIGNED_TEAM_INDEX)
            {
                bool shouldBeMobile = playerPlatform == PlayerPlatform.Mobile;
                string playersName = string.Join(" / ", UserInstanceManager.instance.All().Where(x => x.Team == UNASSIGNED_TEAM_INDEX && x.IsMobile == shouldBeMobile).Select(x => x.PlayerName));
                
                OnTeamSet(teamIndex, playersName, playerPlatform);
            }
            else OnTeamSet(teamIndex, userInstance.PlayerName, playerPlatform);
            

            if (teamIndex == UNASSIGNED_TEAM_INDEX)
            {
                Debug.Log("Team recap:\n" +
                          $"Client id {ownerClientId} unassigned from team index {previousUserTeamIndex}");
            }
            else
            {
                Debug.Log("Team recap :\n" +
                          $"Index : {teamIndex}\n" +
                          $"Pc : {_teams[teamIndex].pcPlayerOwnerClientId}\n" +
                          $"Mobile : {_teams[teamIndex].mobilePlayerOwnerClientId}");
            }
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
            var playerPlatform = user.IsMobile ? PlayerPlatform.Mobile : PlayerPlatform.Pc;
            
            if (IsTeamIndexValid(oldTeam))
            {
                if (oldTeam != UNASSIGNED_TEAM_INDEX) ResetTeamSlot(oldTeam, playerPlatform);
                
                OnTeamSet(oldTeam, playerPlatform == PlayerPlatform.Pc ? DEFAULT_PC_SLOT_TEXT : DEFAULT_MOBILE_SLOT_TEXT, playerPlatform);
            }
            
            if (newTeam != UNASSIGNED_TEAM_INDEX) RegisterToTeamSlotLocal(user.ClientId, newTeam, playerPlatform);
        }
        
        private void ResetTeamSlot(int teamIndex, PlayerPlatform playerPlatform)
        {
            TeamData teamData = _teams[teamIndex];
            if (playerPlatform == PlayerPlatform.Pc) teamData.pcPlayerOwnerClientId = UNASSIGNED_TEAM_INDEX;
            else teamData.mobilePlayerOwnerClientId = UNASSIGNED_TEAM_INDEX;
            _teams[teamIndex] = teamData;
        }

        public bool IsTeamPlayerSlotAvailable(int teamIndex, PlayerPlatform playerPlatform)
        {
            if (teamIndex == UNASSIGNED_TEAM_INDEX) return true;
            
            if (TryGetTeam(teamIndex, out TeamData teamData))
            {
                if (playerPlatform == PlayerPlatform.Pc)
                {
                    return teamData.pcPlayerOwnerClientId == UNASSIGNED_TEAM_INDEX;
                }

                return teamData.mobilePlayerOwnerClientId == UNASSIGNED_TEAM_INDEX;
            }

            return false;
        }

        public bool IsTeamIndexValid(int teamIndex)
        {
            // Valid if teamIndex is in the range of the  or is unassigned
            return (teamIndex >= 0 && teamIndex < _teams.Length) || teamIndex == UNASSIGNED_TEAM_INDEX;
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
            onTeamSet?.Invoke(teamIndex, playerName, playerPlatform);
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
        
        private void OnUserInstanceDespawned_CleanTeam(UserInstance userInstance)
        {
            if (userInstance.Team != UNASSIGNED_TEAM_INDEX)
            {
                TeamData teamData = _teams[userInstance.Team];
                if (userInstance.IsMobile) teamData.mobilePlayerOwnerClientId = UNASSIGNED_TEAM_INDEX;
                else teamData.pcPlayerOwnerClientId = UNASSIGNED_TEAM_INDEX;
                
                _teams[userInstance.Team] = teamData;
            }
        }
    }
}
