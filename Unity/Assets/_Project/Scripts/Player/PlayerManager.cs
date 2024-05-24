using System;
using System.Collections.Generic;
using System.Linq;
using Project._Project.Scripts.Player.States;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Project
{
    public class PlayerManager : NetworkSingleton<PlayerManager>
    {
        // Used to destroy all players when game ends
        [HideInInspector, ServerField] public List<PlayerRefs> players = new();
        
        [SerializeField] private List<Transform> spawnPoints = new();

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

            var spawnPoint = GetSpawnPoint(players.Count);

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
            Debug.LogError("OnDeath");
            OnPlayerDied?.Invoke(refs);
        }

        public void PlacePlayers()
        {
            for(int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                
                if (player is PCPlayerRefs refs)
                {
                    var pos = GetSpawnPoint(i).position;
                    
                    refs.PlayerTransform.GetComponent<NetworkTransform>()
                        .Teleport(pos, Quaternion.identity, Vector3.one);

                    var navMeshAgent = refs.NavMeshAgent;
            
                    navMeshAgent.velocity = Vector3.zero;
                    navMeshAgent.isStopped = true;
                    navMeshAgent.ResetPath();
            
                    navMeshAgent.Warp(pos);
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
    }
}