using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Project
{
    public class PlayerManager : NetworkSingleton<PlayerManager>
    {
        // Used to destroy all players when game ends
        [ServerField] private List<PlayerRefs> _players = new();
        
        [SerializeField] private List<Transform> spawnPoints = new();
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsServer && !IsHost) return;
            
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoaded;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (!IsServer && !IsHost) return;
            if (NetworkManager.Singleton is not { SceneManager: not null }) return;
            
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(ulong clientId, string scene, LoadSceneMode mode)
        {
            if (!UserInstanceManager.instance.TryGetUserInstance((int)clientId, out var user)) return;
            if (!TeamManager.instance.TryGetTeam(user.Team, out var team))
            {
                Debug.LogError($"Team {user.Team} is invalid for user {user.ClientId}");
                return;
            }

            if (!team.HasPC)
            {
                // This should never happen
                Debug.LogError($"Team {user.Team} has no PC player!");
                return;
            }
            
            // user is a netcode client, team is valid, team has pc
            // -> we don't need to check if he is team's pc player
            SpawnPlayerForUser(user, clientId);

            if (team.TryGetUserInstance(PlayerPlatform.Mobile, out var mobileUser))
                SpawnPlayerForUser(mobileUser, clientId);
        }
        public void SpawnPlayerForUser(UserInstance user, ulong clientId)
        { 
            if (!SOCharacter.TryGetCharacter(user.CharacterId, out var characterData)) return;

            var spawnPoint = GetSpawnPoint(_players.Count);

            var obj = Instantiate(characterData.prefab, spawnPoint.position, Quaternion.identity);
            obj.name = "Player " + (user.IsMobile ? "(Mobile) " : "(PC) ") + clientId;
            obj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);

            var refs = obj.GetComponent<PlayerRefs>();
            _players.Add(refs);
            
            refs.ServerInit(user.Team, user.ClientId, characterData);
        }

        private Transform GetSpawnPoint(int index)
        {
            return spawnPoints[index % spawnPoints.Count];
        }
    }
}