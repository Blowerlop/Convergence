using System;
using System.Linq;
using DG.Tweening;
using Project._Project.Scripts.Managers;
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

        private void OnPlayerDied(PlayerRefs refs)
        {
            var players = PlayerManager.instance.players;
            
            var alivePlayers = players.FindAll(p => p is PCPlayerRefs pcPlayerRefs && pcPlayerRefs.StateMachine.currentState is not DeadState);
            
            if (alivePlayers.Count != 1) return;
            
            OnLastPlayerAlive(alivePlayers[0]);
        }

        private void OnLastPlayerAlive(PlayerRefs refs)
        {
            bool endGame = false;

            var pcUser = UserInstanceManager.instance.GetUsersInstance().FirstOrDefault(u => !u.IsMobile && u.Team == refs.TeamIndex);
            
            if (!pcUser)
            {   
                Debug.LogError("No PC user found for team " + refs.TeamIndex);
                return;
            }
            
            pcUser.WinCount.Value++;
            DOVirtual.DelayedCall(2.0f, () => pcUser.WinCount.Sync());
            
            endGame = pcUser.WinCount.Value >= 2;
            
            var mobileUser = UserInstanceManager.instance.GetUsersInstance().FirstOrDefault(u => u.IsMobile && u.Team == refs.TeamIndex);
            if (mobileUser) mobileUser.WinCount.Value++;

            string winnerPlayers = (pcUser == null ? "" : pcUser.PlayerName) + (mobileUser == null ? "" : " & " + mobileUser.PlayerName);
            ShowWinText(winnerPlayers, endGame);

            EndCurrentRound(endGame);
            OnRoundEndedClientRpc(winnerPlayers, endGame);
        }

        [ClientRpc]
        private void OnRoundEndedClientRpc(string winnerNames, bool gameFinished = false)
        {
            if (IsHost) return;
            
            ShowWinText(winnerNames, gameFinished);

            if(gameFinished)
            {
                DOVirtual.DelayedCall(roundEndTime, () =>
                {
                    Netcode_ConnectionManager.Disconnect();
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
                });
            }
        }

        private void ShowWinText(string winnerNames, bool gameFinished = false)
        {
            PlaceholderLabel.instance.SetText($"Team {winnerNames} win " + (gameFinished ? "this game ! " : "this round !"), 1.9f);
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
            
            timer.StartTimerWithUpdateCallback(this, roundStartTime + 1f, (value) =>
            {
                PlaceholderLabel.instance.SetText($"Round starting in {value}");
            }, () =>
            {
                _isGameRunning.Value = true;
                PlaceholderLabel.instance.SetText("Fight !", 1.5f);
            }, ceiled: true);
        }
        
        // Maybe not that great but flemme to netvar
        [ClientRpc]
        private void OnRoundStartClientRpc()
        {
            SoundManager.instance.PlayGlobalSound("CountDown", "timer", SoundManager.EventType.UI);
            
            if (IsHost) return;
            
            Timer timer = new Timer();
            
            timer.StartTimerWithUpdateCallback(this, 4f, (value) =>
            {
                PlaceholderLabel.instance.SetText($"Round starting in {value}");
            }, () =>
            {
                PlaceholderLabel.instance.SetText("Fight !", 1.5f);
            }, ceiled: true);
        }
    }
}
