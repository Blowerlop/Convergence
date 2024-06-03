using Project._Project.Scripts;
using Project._Project.Scripts.Player.States;
using Project._Project.Scripts.StateMachine;
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
        
        [SerializeField] private Camera _deathCamera;
        

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
            if (IsOwner)
            {
                _deathCamera = GameObject.FindGameObjectWithTag(Constants.Tags.Death_Camera)?.GetComponent<Camera>();

                if (IsServer)
                {
                    _refs.StateMachine.SrvOnStateEnter += OwnerOnDeadStateEnter_EnableDeathCamera;
                    _refs.StateMachine.SrvOnStateExit += OwnerOnDeadStateExit_DisableDeathCamera;
                }
                else if (IsClient)
                {
                    _refs.StateMachine.CliOnStateEnter += OwnerOnDeadStateEnter_EnableDeathCamera;
                    _refs.StateMachine.CliOnStateExit += OwnerOnDeadStateExit_DisableDeathCamera;
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _currentAnimation.Reset();
            
            if (IsServer) _stats.Get<HealthStat>().OnValueChanged -= OnHealthChanged_CheckIfDead;
            if (IsOwner)
            {
                if (IsServer)
                {
                    _refs.StateMachine.SrvOnStateEnter -= OwnerOnDeadStateEnter_EnableDeathCamera;
                    _refs.StateMachine.SrvOnStateExit -= OwnerOnDeadStateExit_DisableDeathCamera;
                }
                else if (IsClient)
                {
                    _refs.StateMachine.CliOnStateEnter -=  OwnerOnDeadStateEnter_EnableDeathCamera;
                    _refs.StateMachine.CliOnStateExit -= OwnerOnDeadStateExit_DisableDeathCamera;
                }
            }
        }

        protected override void OnStunned()
        {
            base.OnStunned();
            
            _refs.StateMachine.ChangeStateTo<StunState>();
        }

        protected override void OnUnStunned()
        {
            base.OnUnStunned();
            
            _refs.StateMachine.ChangeStateTo<IdleState>();
        }

        private void Update()
        {
            if (IsServer == false) return;
            
            // Shit
            int animationHash = _refs.NetworkAnimator.Animator.GetNextAnimatorStateInfo(0).shortNameHash;
            if (animationHash == _currentAnimationHash) return;
            if (animationHash == 0) return;
            
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
        
        private void OwnerOnDeadStateEnter_EnableDeathCamera(BaseStateMachineBehaviour currentState)
        {
            Debug.Log("Should enable death camera");
            
            if (currentState is DeadState)
            {
                Debug.Log("Enable death camera");
                _deathCamera.enabled = true;
            }
        }
        
        private void OwnerOnDeadStateExit_DisableDeathCamera(BaseStateMachineBehaviour currentState)
        {
            if (currentState is DeadState)
            {
                _deathCamera.enabled = false;
            }
        }
    }
}
