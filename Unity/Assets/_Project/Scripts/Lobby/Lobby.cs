using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class Lobby : MonoBehaviour
    {
        [SerializeField] private Pager _pager;
        
        private void Start()
        {
            _pager.GoToPage(0, true);
        }

        private void OnEnable()
        {
            TeamManager.instance.onAllPlayersReadyEvent.Subscribe(this, NextPageServerRpc);
        }

        private void OnDisable()
        {
            if (TeamManager.isBeingDestroyed) return;
            
            TeamManager.instance.onAllPlayersReadyEvent.Unsubscribe(NextPageServerRpc);
        }


        [ServerRpc]
        private void NextPageServerRpc()
        {
            NextPageClientRpc();
        }
        
        [ClientRpc]
        private void NextPageClientRpc()
        {
            NextLocal();
        }
        
        private void NextLocal()
        {
            _pager.NextPage();
        }
    }
}
