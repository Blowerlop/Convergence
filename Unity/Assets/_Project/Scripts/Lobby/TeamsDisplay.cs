using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    public class TeamsDisplay : NetworkBehaviour
    {
        private readonly Dictionary<int, Image> _playersAvatar = new Dictionary<int, Image>();
        public Image template;
        [SerializeField] private Sprite _noCharacterSprite;

        
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
            CharacterSelectionUI.onCharacterSelectedEvent.Subscribe(this, SetPlayerCharacterAvatarServerRpc);
        }

        private void OnDisable()
        {
            CharacterSelectionUI.onCharacterSelectedEvent.Unsubscribe(SetPlayerCharacterAvatarServerRpc);
        }
        
        private void OnPlayersReady_DisplayPlayersAvatar()
        {
            if (!IsServer && !IsOwner) return;
            
            DisplayPlayersAvatarServerRpc();
        }

        [ClientRpc]
        private void InstantiateUiClientRpc()
        {
            TeamData[] teams = TeamManager.instance.GetTeams();
            for (int i = 0; i < teams.Length; i++)
            {
                Image instance = Instantiate(template, transform);
                instance.gameObject.SetActive(true);
                _playersAvatar.Add(i, instance);
            }
        }
        
        [ServerRpc]
        private void DisplayPlayersAvatarServerRpc()
        {
            InstantiateUiClientRpc();
            
            var teams = TeamManager.instance.GetTeams();
            
            for (int i = 0; i < teams.Length; i++)
            {
                if (TeamManager.instance.IsTeamPlayerSlotAvailable(i, PlayerPlatform.Pc))
                {
                    SetDefaultCharacterAvatarServerRpc(i);
                }
                else
                {
                    int playerCharacterId = UserInstanceManager.instance.GetUserInstance(teams[i].pcPlayerOwnerClientId).CharacterId;

                    if (SOCharacter.TryGetCharacter(playerCharacterId, out SOCharacter _))
                    {
                        SetPlayerCharacterAvatarClientRpc(i, playerCharacterId);
                    }
                    else
                    {
                        SetDefaultCharacterAvatarServerRpc(i);
                    }
                }
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SetPlayerCharacterAvatarServerRpc(int teamId, int characterId)
        {
            SetPlayerCharacterAvatarClientRpc(teamId, characterId);
        }

        [ClientRpc]
        private void SetPlayerCharacterAvatarClientRpc(int teamId, int characterId)
        {
            SetPlayerCharacterAvatarLocal(teamId, characterId);
        }

        private void SetPlayerCharacterAvatarLocal(int teamId, int characterId)
        {
            _playersAvatar[teamId].sprite = SOCharacter.GetCharacter(characterId).avatar;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SetDefaultCharacterAvatarServerRpc(int teamId)
        {
            SetDefaultCharacterAvatarClientRpc(teamId);
        }

        [ClientRpc]
        private void SetDefaultCharacterAvatarClientRpc(int teamId)
        {
            SetDefaultCharacterAvatarLocal(teamId);
        }

        private void SetDefaultCharacterAvatarLocal(int teamId)
        {
            _playersAvatar[teamId].sprite = _noCharacterSprite;
        }
    }
}
