using Project._Project.Scripts.Player.States;
using Unity.Netcode;

namespace Project
{
    public class PlayerLifeTime : NetworkBehaviour
    {
        private PlayerStateMachineController _stateMachine;
        private PCStats _stats;


        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                PlayerRefs playerRefs = GetComponentInParent<PlayerRefs>();
                _stats = (PCStats)playerRefs.Stats;
                _stateMachine = playerRefs.StateMachine;
                            
                _stats.OnHealthChanged += OnHealthChanged_CheckIfDead;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (IsServer) _stats.OnHealthChanged -= OnHealthChanged_CheckIfDead;
        }
    

        private void OnHealthChanged_CheckIfDead(int currentHealth, int maxHealth)
        {
            if (currentHealth <= 0) _stateMachine.ChangeState(_stateMachine.deadState);
        }
    }
}
