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

        private void OnEnable()
        {
            TeamManager.instance.onTeamSetEvent.Subscribe(this, OnTeamSet_UpdateButtonText);
        }

        private void OnDisable()
        {
            if (TeamManager.isBeingDestroyed == false)
            {
                TeamManager.instance.onTeamSetEvent.Unsubscribe(OnTeamSet_UpdateButtonText);
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
            UpdatePcButtonTextLocal(clientName);
        }

        private void UpdatePcButtonTextLocal(string clientName)
        {
            _pcButtonText.text = clientName;
        }
        
        [ClientRpc]
        private void UpdateMobileButtonTextClientRpc(string clientName)
        {
            UpdateMobileButtonTextLocal(clientName);
        }
        
        private void UpdateMobileButtonTextLocal(string clientName)
        {
            _mobileButtonText.text = clientName;
        }
    }
}
