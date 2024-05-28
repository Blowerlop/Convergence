using System;
using System.Collections.Generic;
using System.Linq;
using Project._Project.Scripts;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project
{
    public class PlayerManager : NetworkSingleton<PlayerManager>
    {
        // Used to destroy all players when game ends
        [HideInInspector, ServerField] public List<PlayerRefs> players = new();
        
        [SerializeField] private List<Transform> spawnPoints = new();

        public static event Action OnAllPlayersReady;
        public static event Action<PlayerRefs> OnPlayerDied;
        
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

            var pcCount = players.Count(player => player is PCPlayerRefs);
            
            var spawnPoint = GetSpawnPoint(pcCount);

            var obj = Instantiate(characterData.prefab, spawnPoint.position, Quaternion.identity);
            obj.name = "Player " + (user.IsMobile ? "(Mobile) " : "(PC) ") + clientId;
            obj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);

            var refs = obj.GetComponent<PlayerRefs>();
            players.Add(refs);
            
            refs.ServerInit(user.Team, user.ClientId, characterData);
        }

        private Transform GetSpawnPoint(int index)
        {
            return spawnPoints[index % spawnPoints.Count];
        }
        
        public void OnDeath(PlayerRefs refs)
        {
            OnPlayerDied?.Invoke(refs);
        }

        public void PlacePlayers()
        {
            var pcCount = 0;

            foreach (var player in players)
            {
                if (player is PCPlayerRefs refs)
                {
                    var temp = pcCount;
                    
                    var pos = GetSpawnPoint(temp).position;
                    
                    refs.PlayerTransform.GetComponent<NetworkTransform>()
                        .Teleport(pos, Quaternion.identity, Vector3.one);

                    var navMeshAgent = refs.NavMeshAgent;
            
                    navMeshAgent.velocity = Vector3.zero;
                    navMeshAgent.isStopped = true;
                    navMeshAgent.ResetPath();
            
                    navMeshAgent.Warp(pos);
                    pcCount++;
                }
            }
        }

        public void ResetPlayers()
        {
            foreach (var player in players)
            {
                player.SrvResetPlayer();
            }
        }

        public void SetPlayerReady()
        {
            var users = UserInstanceManager.instance.GetUsersInstance();
            
            // All users have a player linked
            if (users.All(user => user.LinkedPlayer))
            {
                OnAllPlayersReady?.Invoke();
            }
        }

        #if UNITY_EDITOR
        
        [ConsoleCommand("spawn_dummy", "Spawns a dummy at position 5, 0, 5.")]
        public static void DebugSpawnDummy()
        {
            var dummy = Resources.LoadAll<Dummy>("");
            
            Debug.Log($"Found {dummy.Length} dummy in resources.");
            
            if (dummy.Length > 0)
            {
                Instantiate(dummy[0], new Vector3(5, 0, 5), Quaternion.identity).GetComponent<NetworkObject>().Spawn();
                Debug.Log("Dummy spawned!");
            }
        }
        
        #endif
    }
}