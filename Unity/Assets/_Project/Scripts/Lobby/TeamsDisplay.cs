using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    public class TeamsDisplay : NetworkBehaviour
    {
        private readonly Dictionary<int, Image> _teamDisplays = new Dictionary<int, Image>();
        public Image template;

        
        private void Start()
        {
            TeamManager.instance.onAllPlayersReadyEvent.Subscribe(this, OnPlayersReady_DisplayPlayersAvatar);
        }

        private void OnDestroy()
        {
            if (TeamManager.isBeingDestroyed == false)
            {
                TeamManager.instance.onAllPlayersReadyEvent.Unsubscribe(OnPlayersReady_DisplayPlayersAvatar);
            }
        }
        
        private void OnEnable()
        {
            CharacterSelectionUI.onCharacterSelectedEvent.Subscribe(this, SetAvatarServerRpc);
        }

        private void OnDisable()
        {
            CharacterSelectionUI.onCharacterSelectedEvent.Unsubscribe(SetAvatarServerRpc);
        }
        
        private void OnPlayersReady_DisplayPlayersAvatar()
        {
            if (!IsServer && !IsOwner) return;
            
            DisplayPlayersAvatarServerRpc();
        }
        
        [ServerRpc]
        private void DisplayPlayersAvatarServerRpc()
        {
            var teams = TeamManager.instance.GetTeams();
            
            for (int i = 0; i < teams.Length; i++)
            {
                DisplayPlayerAvatarClientRpc(teams[i].pcPlayerOwnerClientId);
            }
        }

        [ClientRpc]
        private void DisplayPlayerAvatarClientRpc(int playerId)
        {
            DisplayPlayerAvatarLocal(playerId);
        }

        private void DisplayPlayerAvatarLocal(int playerId)
        {
            Image instance = Instantiate(template, transform);
            _teamDisplays.Add(playerId, instance);
            instance.gameObject.SetActive(true);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetAvatarServerRpc(int playerId, int characterId)
        {
            SetAvatarClientRpc(playerId, characterId);
        }

        [ClientRpc]
        private void SetAvatarClientRpc(int playerId, int characterId)
        {
            SetAvatarLocal(playerId, characterId);
        }

        private void SetAvatarLocal(int playerId, int characterId)
        {
            _teamDisplays[playerId].sprite = SOCharacter.GetCharacter(characterId).avatar;
        }
    }
}
