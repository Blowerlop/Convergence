using System;
using Project._Project.Scripts.Player.States;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class Gameloop : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            
            PlayerManager.OnPlayerDied += OnPlayerDied;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            
            PlayerManager.OnPlayerDied -= OnPlayerDied;
        }

        private void OnPlayerDied(PlayerRefs refs)
        {
            Debug.LogError("OnPlayerDied");
            var players = PlayerManager.instance.players;
            
            var alivePlayers = players.FindAll(p => p.GetPC().StateMachine.currentState is not DeadState);

            Debug.LogError($"{alivePlayers.Count}");
            
            if (alivePlayers.Count != 1) return;
            
            OnLastPlayerAlive(alivePlayers[0]);
        }

        private void OnLastPlayerAlive(PlayerRefs refs)
        {
            Debug.LogError($"preeee OnLastPlayerAlive");
            
            if (!TeamManager.instance.TryGetTeam(refs.TeamIndex, out var team))
            {
                // ???
                return;
            }

            Debug.LogError($"OnLastPlayerAlive");

            if (team.TryGetUserInstance(PlayerPlatform.Pc, out var pcUser))
            {
                
                pcUser.WinCount.Value++;
            }
            else
            {
                Debug.LogError("No PC user");
            }
            
            Debug.LogError(team.HasPC);
            Debug.LogError(team.mobilePlayerOwnerClientId);
            Debug.LogError(team.pcPlayerOwnerClientId);
            Debug.LogError(team.HasMobile);
            
            if (team.TryGetUserInstance(PlayerPlatform.Mobile, out var mobileUser))
                mobileUser.WinCount.Value++;
            
            StartNewRound();
        }

        private void StartNewRound()
        {
            Debug.LogError($"StartNewRound");
            
            var playerManager = PlayerManager.instance;
            
            playerManager.ResetPlayers();
            playerManager.PlacePlayers();
        }
    }
}
