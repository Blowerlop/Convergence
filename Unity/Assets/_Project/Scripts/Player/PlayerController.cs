using System;
using Project._Project.Scripts;
using Project.Spells;
using UnityEngine;

namespace Project
{
    public class PlayerController : Entity
    {
        [SerializeField] private PCPlayerRefs _refs;

        public override int TeamIndex => _refs.TeamIndex;

        private void Start()
        {
            SpellManager.OnChannelingStarted += OnChannelingStarted;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            SpellManager.OnChannelingStarted -= OnChannelingStarted;
        }
        
        private void OnChannelingStarted(PlayerRefs player, Vector3 direction)
        {
            if (player != _refs) return;
            
            _refs.PlayerTransform.rotation = Quaternion.LookRotation(direction);
        }

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
            if (currentHealth <= 0)
            {
                _refs.StateMachine.ChangeState(_refs.StateMachine.deadState);
            }
        }
    }
}
