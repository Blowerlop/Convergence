using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    public class TeamSelectionUI : NetworkBehaviour
    {
        [SerializeField, Min(-1), Tooltip("-1 mean that the team has not been set")] private int _teamIndex = -1;
        private Button _button;
        private TMP_Text _buttonText;
        
        #if UNITY_EDITOR
        [ClearOnReload(true)] private static List<int> _commonTeamIndex = new List<int>((int)TeamManager.MAX_TEAM);
        #endif


        private void Awake()
        {
            _button = GetComponent<Button>();
            _buttonText = _button.GetComponentInChildren<TMP_Text>();
        }

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
                Debug.LogError($"{countSameOccurence} teams have the same team index : {_teamIndex}");
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
            UserInstance userInstance = UserInstanceManager.instance.GetUserInstance(ownerClientId);
            if (userInstance == null)
            {
                Debug.LogError("User instance is null");
                return;
            }

            TeamManager.instance.TrySetTeam(ownerClientId, teamIndex, PlayerPlatform.Pc);
        }

        private void OnTeamSet_UpdateButtonText(int teamIndex, string clientName)
        {
            if (teamIndex != _teamIndex) return;
            
            // Update the UI on the server to make the debugging easier
            UpdateButtonTextLocal(clientName);
            UpdateButtonTextClientRpc(clientName);
        }
        
        [ClientRpc]
        private void UpdateButtonTextClientRpc(string clientName)
        {
            UpdateButtonTextLocal(clientName);
        }

        private void UpdateButtonTextLocal(string clientName)
        {
            _buttonText.text = clientName;
        }
    }
}
