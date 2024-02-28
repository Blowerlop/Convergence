using Project._Project.Scripts;
using UnityEngine;

namespace Project
{
    public class PlayerController : Entity
    {
        [SerializeField] private PlayerRefs _refs;

        public override int TeamIndex => _refs.TeamIndex;


        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _stats.health.OnValueChanged += OnHealthChanged_CheckIfDead;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (IsServer) _stats.health.OnValueChanged -= OnHealthChanged_CheckIfDead;
        }
    

        private void OnHealthChanged_CheckIfDead(int currentHealth, int maxHealth)
        {
            if (currentHealth <= 0) _refs.StateMachine.ChangeState(_refs.StateMachine.deadState);
        }
    }
}
