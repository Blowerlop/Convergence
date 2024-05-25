using System;
using DG.Tweening;
using Project._Project.Scripts.Player.States;
using Project.Spells;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class Gameloop : NetworkSingleton<Gameloop>
    {
        private readonly NetworkVariable<bool> _isGameRunning = new NetworkVariable<bool>(false);
        
        public static bool IsGameRunning => instance._isGameRunning.Value;
        
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            
            PlayerManager.OnPlayerDied += OnPlayerDied;
            PlayerManager.OnAllPlayersReady += StartNewRound;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            
            PlayerManager.OnPlayerDied -= OnPlayerDied;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                EndCurrentRound();
            }
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
            
            if (team.TryGetUserInstance(PlayerPlatform.Mobile, out var mobileUser))
                mobileUser.WinCount.Value++;
            
            EndCurrentRound();
        }

        private void EndCurrentRound()
        {
            _isGameRunning.Value = false;
            
            DOVirtual.DelayedCall(3, () =>
            {
                ResetRound();
                StartNewRound();
            });
        }

        private void ResetRound()
        {
            var spellManager = SpellManager.instance;
            
            spellManager.SrvResetSpells();
            spellManager.SrvResetCasts();
            
            var playerManager = PlayerManager.instance;
            
            playerManager.ResetPlayers();
            playerManager.PlacePlayers();
        }
        
        private void StartNewRound()
        {
            DOVirtual.DelayedCall(5f, () =>
            {
                _isGameRunning.Value = true;
            });
        }
    }
}
