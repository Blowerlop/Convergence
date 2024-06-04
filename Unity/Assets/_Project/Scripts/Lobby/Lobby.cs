using System;
using System.Linq;
using System.Threading.Tasks;
using Project.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project
{
    public enum ELobbyState
    {
        None,
        TeamSelection,
        CharacterSelection,
        Game
    }
    
    public class Lobby : NetworkSingleton<Lobby>
    {
        [SerializeField] private Pager _pager;
        private ELobbyState _lobbyState = ELobbyState.TeamSelection;
        public ELobbyState lobbyState
        {
            get => _lobbyState;
            set
            {
                _lobbyState = value;
                OnStateChange?.Invoke(value);
                Debug.Log("Lobby state changed to: " + value);
            }
        }
        public readonly Action onAllPlayersReadyEvent;
        public event Action<ELobbyState> OnStateChange;

        [ClearOnReload] private static bool CanStartSolo;
        
        private void Start()
        {
            GoToTeamSelectionPage();
        }
        
        public void SetPlayerReadyState(bool state)
        {
            SetPlayerReadyStateServerRpc((int)NetworkManager.Singleton.LocalClientId, state);
        }

        public void TogglePlayerReadyState()
        {
            SetPlayerReadyStateServerRpc((int)NetworkManager.Singleton.LocalClientId, !UserInstance.Me.IsReady);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SetPlayerReadyStateServerRpc(int playerId, bool state)
        {

            UserInstance userInstance = UserInstanceManager.instance.GetUserInstance(playerId);
            if (userInstance == null)
            {
                Debug.LogError("User instance is null");
                return;
            }
            

            userInstance.SrvSetIsReady(state);

            if (userInstance.IsReady)
            {
                CheckIfAllPlayersReady();
            }
        }

        private void CheckIfAllPlayersReady()
        {
            var readyCount = UserInstanceManager.instance.GetUsersInstance().Count(x => x.IsReady || x.IsMobile);
            
            if (readyCount == UserInstanceManager.instance.count && (CanStartSolo || readyCount > 1))
            {
                OnAllPlayersReady();
            }
        }
        
        private void OnAllPlayersReady()
        {
            switch (lobbyState)
            {
                case ELobbyState.TeamSelection:
                    GoToCharacterSelectionPage();
                    break;
                
                case ELobbyState.CharacterSelection:
                    GoToGameScene();
                    break;
                
                case ELobbyState.Game:
                    break;
                
                case ELobbyState.None:
                default:
                    Debug.LogError("Lobby invalid state");
                    break;
            }

            onAllPlayersReadyEvent?.Invoke();
        }

        // Don't need to be a RPC because it will be executed at the Start
        private void GoToTeamSelectionPage()
        {
            _pager.GoToPage(0);
            lobbyState = ELobbyState.TeamSelection;
        }

        private async void GoToCharacterSelectionPage()
        {
            foreach (UserInstance x in UserInstanceManager.instance.GetUsersInstance())
            {
                await Task.Delay(100);
                x.SrvSetIsReady(false);
            }

            if (IsServer)
            {
                GoToCharacterSelectionPageLocal();
            }
            
            GoToCharacterSelectionPageClientRpc();
        }
        
        [ClientRpc]
        private void GoToCharacterSelectionPageClientRpc()
        {
            GoToCharacterSelectionPageLocal();
        }

        private void GoToCharacterSelectionPageLocal()
        {
            _pager.GoToPage(1);
            lobbyState = ELobbyState.CharacterSelection;
        }

        private void GoToGameScene()
        {
            Project.SceneManager.Network_LoadSceneAsync("Spell", LoadSceneMode.Single, new LoadingScreenParameters(null, Color.black));
        }
        
        [ConsoleCommand("start_solo", "Can launch a game solo if this is true")]
        public static void SetCanStartSolo(bool value)
        {
            CanStartSolo = value;
        }
    }
}
