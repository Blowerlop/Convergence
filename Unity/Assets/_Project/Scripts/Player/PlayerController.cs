using System;
using Project._Project.Scripts;
using Project._Project.Scripts.Player.States;
using Project.Extensions;
using Project.Spells;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    public class PlayerController : Entity
    {
        [SerializeField] private PCPlayerRefs _refs;

        private int _currentAnimationHash;
        [ShowInInspector] private GRPC_NetworkVariable<int> _currentAnimation  = new GRPC_NetworkVariable<int>("CurrentAnimation");

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
            base.OnNetworkSpawn();
            
            _currentAnimation.Initialize();
            
            if (IsServer)
            {
                if (_stats.isInitialized)
                {
                    OnStatsInitialized_HookHealth();
                }
                else _stats.OnStatsInitialized += OnStatsInitialized_HookHealth;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _currentAnimation.Reset();
            
            if (IsServer) _stats.Get<HealthStat>().OnValueChanged -= OnHealthChanged_CheckIfDead;
        }
        
        private void Update()
        {
            if (IsServer == false) return;
            
            // Shit
            int animationHash = _refs.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            if (animationHash == _currentAnimationHash) return;
            
            _currentAnimationHash = animationHash;
            _currentAnimation.Value = AnimatorStates.grpcHash[_currentAnimationHash];
            Debug.Log("Update current animation : " + _currentAnimation.Value);
        }


        private void OnHealthChanged_CheckIfDead(int currentHealth, int maxHealth)
        {
            if (currentHealth <= 0)
            {
                _refs.StateMachine.ChangeStateTo<DeadState>();
            }
        }

        private void OnStatsInitialized_HookHealth()
        {
            _stats.Get<HealthStat>().OnValueChanged += OnHealthChanged_CheckIfDead;
            _stats.OnStatsInitialized -= OnStatsInitialized_HookHealth;
        }
    }
}
