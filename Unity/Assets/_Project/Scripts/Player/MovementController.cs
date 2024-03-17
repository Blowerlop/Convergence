using System;
using Project._Project.Scripts.Player.States;
using Project.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Project
{
    public class MovementController : NetworkBehaviour
    {
        private PCPlayerRefs _playerRefs;
        
        private Transform _player;
        private PlayerStateMachineController _stateMachineController;
        
        private NavMeshAgent _agent;

        private const float _DESTINATION_REACHED_OFFSET = 0.1f;
        
        public event Action OnPositionReached;
        
        private void Awake()
        {
            _stateMachineController = GetComponentInParent<PlayerStateMachineController>();
        }

        private void Start()
        {
            if (!IsServer) return;
            
            // Can't put this in OnNetworkSpawn because stats
            // are initialized after in Entity.ServerInit
            
            var moveSpeed = _playerRefs.Entity.Stats.Get<MoveSpeedStat>();

            OnSpeedChanged(moveSpeed.value, moveSpeed.maxValue);
            moveSpeed.OnValueChanged += OnSpeedChanged;
        }

        public override void OnNetworkSpawn()
        {
            enabled = IsServer;
            
            _playerRefs = GetComponentInParent<PCPlayerRefs>();
            NavMeshAgent agent = _playerRefs.NavMeshAgent;
            
            if (IsServer)
            {
                _player = _playerRefs.PlayerTransform;

                _agent = agent;
                _agent.updatePosition = false;
                _agent.updateRotation = false;
                OnPositionReached += OnPositionReached_UpdateState;
            }
            else
            {
                agent.enabled = false;
            }

            if (IsOwner)
            {
                _playerRefs.PlayerMouse.OnMouseClick += TryGoToPosition;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (IsOwner)
            {
                PCPlayerRefs playerRefs = GetComponentInParent<PCPlayerRefs>();
                playerRefs.PlayerMouse.OnMouseClick -= TryGoToPosition;
            }
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            
            OnPositionReached -= OnPositionReached_UpdateState;
        }
        
        
        private void Update()
        {
            if (_stateMachineController.currentState is not MoveState) return;
            
            if (Math.Abs(_agent.remainingDistance - _agent.stoppingDistance) < _DESTINATION_REACHED_OFFSET)
            {
                OnPositionReached?.Invoke();
            }
            else
            {
                _player.rotation = Quaternion.LookRotation((_agent.nextPosition - transform.position).ResetAxis(EAxis.Y).normalized);
                _player.position = _agent.nextPosition;
            }
        }

        private void TryGoToPosition(RaycastHit hitInfo, int layer)
        {
            if (layer == Constants.Layers.GroundIndex)
            {
                GoToServerRpc(hitInfo.point);
            }
        }

        [ServerRpc]
        private void GoToServerRpc(Vector3 position)
        {
            SrvGoTo(position);
        }

        [Server]
        public void SrvGoTo(Vector3 position)
        {
            if (_stateMachineController.currentState is not MoveState && _stateMachineController.CanChangeStateTo(_stateMachineController.moveState))
            {
                _stateMachineController.ChangeState(_stateMachineController.moveState);
            }

            if (_stateMachineController.currentState is MoveState)
            {
                _agent.SetDestination(position);
            }
        }

        private void OnPositionReached_UpdateState()
        {
            if (_stateMachineController.CanChangeStateTo(_stateMachineController.idleState))
            {
                _stateMachineController.ChangeState(_stateMachineController.idleState);
            }
        }
        
        [Server]
        private void OnSpeedChanged(int current, int max)
        {
            _agent.speed = current;
        }
    }
}
