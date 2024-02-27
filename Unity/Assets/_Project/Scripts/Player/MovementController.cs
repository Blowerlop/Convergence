using System;
using Project._Project.Scripts.Player.States;
using Project.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Project
{
    public class MovementController : NetworkBehaviour
    {
        private Transform _player;
        private PlayerStateMachineController _stateMachineController;
        
        private Camera _camera;
        private const int _GROUND_LAYER_MASK = Constants.LayersMask.Ground;
        private NavMeshAgent _agent;

        private const float _DESTINATION_REACHED_OFFSET = 0.1f;
        
        public event Action OnPositionReached;

        
        private void Awake()
        {
            _camera = Camera.main;
            _stateMachineController = GetComponentInParent<PlayerStateMachineController>();
        }

        public override void OnNetworkSpawn()
        {
            enabled = IsServer;
            
            PCPlayerRefs playerRefs = GetComponentInParent<PCPlayerRefs>();
            NavMeshAgent agent = playerRefs.NavMeshAgent; 
                
            if (IsServer)
            {
                _player = playerRefs.PlayerTransform;

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
                InputManager.instance.onMouseButton1.started += TryGoToPosition;
            }
        } 

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (IsOwner && InputManager.IsInstanceAlive())
            {
                InputManager.instance.onMouseButton1.started -= TryGoToPosition;
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

        private void TryGoToPosition(InputAction.CallbackContext _)
        {
            if (Utilities.GetMouseWorldPosition(_camera, _GROUND_LAYER_MASK, out Vector3 position))
            {
                GoToServerRpc(position);
            }
        }

        [ServerRpc]
        private void GoToServerRpc(Vector3 position)
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
    }
}
