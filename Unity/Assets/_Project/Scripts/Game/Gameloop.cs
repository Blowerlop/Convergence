using System;
using System.Linq;
using DG.Tweening;
using Project._Project.Scripts.Player.States;
using Project.Spells;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project
{
    public class Gameloop : NetworkSingleton<Gameloop>
    {
        private readonly NetworkVariable<bool> _isGameRunning = new NetworkVariable<bool>(false);

        public static bool IsGameRunning => instance._isGameRunning.Value;

        [SerializeField] private float roundEndTime = 2f;
        [SerializeField] private float roundStartTime = 3f;
        
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            
            PlayerManager.OnPlayerDied += OnPlayerDied;
            PlayerManager.OnAllPlayersReady += StartNewRound;
        }

        public override void OnNetworkDespawn()
        {
            PlayerManager.OnPlayerDied -= OnPlayerDied;
            PlayerManager.OnAllPlayersReady -= StartNewRound;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                EndCurrentRound(false);
            }
        }

        private void OnPlayerDied(PlayerRefs refs)
        {
            var players = PlayerManager.instance.players;
            
            var alivePlayers = players.FindAll(p => p is PCPlayerRefs pcPlayerRefs && pcPlayerRefs.StateMachine.currentState is not DeadState);
            
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
            
            bool endGame = false;
            
            if (team.TryGetUserInstance(PlayerPlatform.Pc, out var pcUser))
            {
                pcUser.WinCount.Value++;
                DOVirtual.DelayedCall(2.0f, () => pcUser.WinCount.Sync());
                
                
                endGame = pcUser.WinCount.Value >= 2;
            }
            
            if (team.TryGetUserInstance(PlayerPlatform.Mobile, out var mobileUser))
                mobileUser.WinCount.Value++;
            
            ShowWinText(refs.TeamIndex);
            
            EndCurrentRound(endGame);
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

        private void EndCurrentRound(bool endGame)
        {
            _isGameRunning.Value = false;
            
            DOVirtual.DelayedCall(roundEndTime, () =>
            {
                if (endGame)
                {
                    // TODO: Need to reset some managers
                    SceneManager.Network_LoadSceneAsync("Lobby", LoadSceneMode.Single, new LoadingScreenParameters(null, Color.black));
                    return;
                }
                
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
            
            timer.StartTimerWithUpdateCallback(this, roundStartTime, (value) =>
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
