using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class TeamSelectionUI : NetworkBehaviour
    {
        [SerializeField, Min(-1), Tooltip("-1 mean that the team has not been set")] private int _teamIndex = -1;
        [SerializeField] private TMP_Text _pcButtonText;
        [SerializeField] private TMP_Text _mobileButtonText;
        
        #if UNITY_EDITOR
        // Used for debug purpose
        // It permit to advertise if two teams have the same index
        [ClearOnReload(assignNewTypeInstance: true)] private static readonly List<int> _commonTeamIndex = new List<int>((int)TeamManager.MAX_TEAM);
        #endif

        
        private void Start()
        {
            CheckTeamIndexValidity();
        }

        public override void OnNetworkSpawn()
        {
            TeamManager.instance.onTeamSetEvent.Subscribe(this, OnTeamSet_UpdateButtonText);

            // ONLY IN EDITOR AND FOR A SPECIFIC USE CASE.
            // BECAUSE RIGHT NOW IN TESTING THIS NETWORK SPAWN MIGHT FIRED FIRST, WE HAVE A NULL REF ON OUR USERINSTANCE.
            // BUT IN THE FINAL GAME, THE USER INSTANCE WILL BE THE FIRST THING EVER FIRED IN THE NETWORK (NORMALLY)
            #if UNITY_EDITOR
             Utilities.StartWaitForFramesAndDoActionCoroutine(this, 1,
                () => UserInstance.Me._networkIsReady.OnValueChanged += OnPlayerReady_UpdateButtonTextColor);
             #else
             UserInstance.Me._networkIsReady.OnValueChanged += OnPlayerReady_UpdateButtonTextColor);
             #endif

        }

        public override void OnNetworkDespawn()
        {
            if (TeamManager.isBeingDestroyed == false)
            {
                TeamManager.instance.onTeamSetEvent.Unsubscribe(OnTeamSet_UpdateButtonText);
            }

            if (UserInstance.Me != null)
            {
                UserInstance.Me._networkIsReady.OnValueChanged -= OnPlayerReady_UpdateButtonTextColor;
            }   
        }

        private void CheckTeamIndexValidity()
        {
            if (TeamManager.instance.IsTeamIndexValid(_teamIndex) == false)
            {
                Debug.LogError("Team index is invalid");
            }

            #if UNITY_EDITOR
            _commonTeamIndex.Add(_teamIndex);
            int countSameOccurence = _commonTeamIndex.Count(x => x == _teamIndex);
            if (countSameOccurence > 1)
            {
                Debug.LogError($"{countSameOccurence} teams have the team index {_teamIndex}");
            }
            #endif
        }
        
        public void SetTeamServerRpcc()
        {
            #if UNITY_EDITOR
            if (FU_GRPC_NetworkManager.instance.isConnected)
            {
                TeamManager.instance.FU_Write(NetworkManager.Singleton.LocalClientId, _teamIndex);
                return;
            }
            #endif
            SetTeamServerRpc(_teamIndex, NetworkManager.Singleton.LocalClientId);
            
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetTeamServerRpc(int teamIndex, ulong ownerClientId)
        {
            UserInstance userInstance = UserInstanceManager.instance.GetUserInstance((int)ownerClientId);
            if (userInstance == null)
            {
                Debug.LogError("User instance is null");
                return;
            }

            TeamManager.instance.TrySetTeam((int)ownerClientId, teamIndex, PlayerPlatform.Pc);
        }

        private void OnTeamSet_UpdateButtonText(int teamIndex, string clientName, PlayerPlatform playerPlatform)
        {
            if (teamIndex != _teamIndex) return;

            if (playerPlatform == PlayerPlatform.Pc)
            {
                UpdatePcButtonTextClientRpc(clientName);
            }
            else
            {
                UpdateMobileButtonTextClientRpc(clientName);
            }
            
        }
        
        [ClientRpc]
        private void UpdatePcButtonTextClientRpc(string clientName)
        {
            _pcButtonText.text = clientName;
        }

        [ClientRpc]
        private void UpdateMobileButtonTextClientRpc(string clientName)
        {
            _mobileButtonText.text = clientName;
        }

        private void OnPlayerReady_UpdateButtonTextColor(bool _, bool readyState)
        {
            Debug.LogError("HERE");
            OnPlayerReady_UpdateButtonTextColorServerRpc((int)NetworkManager.Singleton.LocalClientId, readyState);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void OnPlayerReady_UpdateButtonTextColorServerRpc(int clientId, bool readyState)
        {
            TeamData teamData = TeamManager.instance.GetTeamData(_teamIndex);

            if (teamData.pcPlayerOwnerClientId == clientId)
            {
                UpdatePcButtonTextColorClientRpc(_teamIndex, readyState);
            }
            else if (teamData.mobilePlayerOwnerClientId == clientId)
            {
                UpdateMobileButtonTextColorClientRpc(_teamIndex, readyState);
            }
        }

        [ClientRpc]
        private void UpdatePcButtonTextColorClientRpc(int teamIndex, bool state)
        {
            if (teamIndex != _teamIndex) return;
            
            _pcButtonText.color = state ? Color.green : Color.black;
        }
        
        [ClientRpc]
        private void UpdateMobileButtonTextColorClientRpc(int teamIndex, bool state)
        {
            if (teamIndex != _teamIndex) return;
            
            _mobileButtonText.color = state ? Color.green : Color.black;
        }
    }
}
