using System;
using System.Collections.Generic;
using System.Linq;
using Project.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    public class TeamsDisplay : NetworkBehaviour
    {
        private readonly Dictionary<int, Image> _playersAvatar = new Dictionary<int, Image>();
        [SerializeField] private Sprite _defaultSprite;
        [SerializeField] private Image _template;


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            Init();
        }
        
        
        private void Init()
        {
            SpawnUi();
            SetPlayersAvatar();
        }

        private void SpawnUi()
        {
            for (int i = 0; i < TeamManager.MAX_TEAM; i++)
            {
                Image instance = Instantiate(_template, transform);
                _playersAvatar.Add(i, instance);
            }
        }

        private void SetPlayersAvatar()
        {
            var teams = TeamManager.instance.GetTeamsData();
            
            for (int i = 0; i < TeamManager.MAX_TEAM; i++)
            {
                if (TeamManager.instance.IsTeamPlayerSlotAvailable(i, PlayerPlatform.Pc))
                {
                    GetByTeamId(i).sprite = _defaultSprite;
                }
                else
                {
                    int playerCharacterId = UserInstanceManager.instance.GetUserInstance(teams[i].pcPlayerOwnerClientId).CharacterId;

                    GetByTeamId(i).sprite = SOCharacter.TryGetCharacter(playerCharacterId, out SOCharacter characterData) ? characterData.avatar : _defaultSprite;
                }
            }
        }

        private Image GetByTeamId(int teamId) => _playersAvatar[teamId];

        private Image GetByPlayerId(int playerId)
        {
            var teams = TeamManager.instance.GetTeamsData();
            return GetByTeamId(teams.FindIndex(x => x.pcPlayerOwnerClientId == playerId));
        }

        public void SetAvatar(int playerId, int characterId)
        {
            Image player = GetByPlayerId(playerId);

            if (SOCharacter.TryGetCharacter(characterId, out SOCharacter characterData))
            {
                player.sprite = characterData.avatar;
            }
            else Debug.LogError("No character with id " + characterId);
        }
    }
}
