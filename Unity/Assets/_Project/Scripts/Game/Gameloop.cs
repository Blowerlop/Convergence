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
            var players = PlayerManager.instance.players;
            
            var alivePlayers = players.FindAll(p => p.GetPC().StateMachine.currentState is not DeadState);
            
            if (alivePlayers.Count != 1) return;
            
            OnLastPlayerAlive(alivePlayers[0]);
        }

        private void OnLastPlayerAlive(PlayerRefs refs)
        {
            if (!TeamManager.instance.TryGetTeam(refs.TeamIndex, out var team))
            {
                // ???
                return;
            }
            
            if (team.TryGetUserInstance(PlayerPlatform.Pc, out var pcUser))
            {
                pcUser.WinCount.Value++;
            }
            
            if (team.TryGetUserInstance(PlayerPlatform.Mobile, out var mobileUser))
                mobileUser.WinCount.Value++;
            
            ShowWinText(refs.TeamIndex);
            
            EndCurrentRound();
            OnRoundEndedClientRpc(refs.TeamIndex);
        }

        [ClientRpc]
        private void OnRoundEndedClientRpc(int teamIndex)
        {
            if (IsHost) return;
            
            ShowWinText(teamIndex);
        }

        private void ShowWinText(int teamIndex)
        {
            PlaceholderLabel.instance.SetText($"Team {teamIndex} wins this round!", 1.9f);
        }

        private void EndCurrentRound()
        {
            _isGameRunning.Value = false;
            
            DOVirtual.DelayedCall(2, () =>
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
            Timer timer = new Timer();
            
            OnRoundStartClientRpc();
            
            timer.StartTimerWithUpdateCallback(this, 3f, (value) =>
            {
                PlaceholderLabel.instance.SetText($"Round starting in {value}");
            }, () =>
            {
                _isGameRunning.Value = true;
                PlaceholderLabel.instance.SetText("");
            }, ceiled: true);
        }
        
        // Maybe not that great but flemme to netvar
        [ClientRpc]
        private void OnRoundStartClientRpc()
        {
            if (IsHost) return;
            
            Timer timer = new Timer();
            
            timer.StartTimerWithUpdateCallback(this, 3f, (value) =>
            {
                PlaceholderLabel.instance.SetText($"Round starting in {value}");
            }, () =>
            {
                PlaceholderLabel.instance.SetText("");
            }, ceiled: true);
        }
    }
}
