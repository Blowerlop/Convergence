using System;
using Project.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Project
{
    public class MovementController : NetworkBehaviour
    {
        private Camera _camera;
        private const int _GROUND_LAYER_MASK = Constants.LayersMask.Ground;
        private NavMeshAgent _agent;
        public PlayerState state;

        private Coroutine _movementLerpCoroutine;
        private Coroutine _rotationLerpCoroutine;

        private const float _DESTINATION_REACHED_OFFSET = 0.1f;
        
        public static event Action OnPositionReached;

        
        private void Awake()
        {
            _camera = Camera.main;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            enabled = IsOwner;
            if (IsServer)
            {
                _agent = GetComponent<NavMeshAgent>();
                _agent.updatePosition = false;
                _agent.updateRotation = false;
                state = new PlayerIdleState();
                state.StartState(this);
                OnPositionReached += OnPositionReached_UpdateState;
            }
            else
                GetComponent<NavMeshAgent>().enabled = false;
        }

        private void OnEnable()
        {
            InputManager.instance.onMouseButton1.started += TryGoToPosition;
        }
        
        private void OnDisable()
        {
            if (InputManager.IsInstanceAlive())
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
            if (state is PlayerMoveState)
            {
                if (state is PlayerMoveState && Math.Abs(_agent.remainingDistance - _agent.stoppingDistance) < _DESTINATION_REACHED_OFFSET)
                {
                    OnPositionReached?.Invoke();
                }
                else
                {
                    transform.rotation = Quaternion.LookRotation((_agent.nextPosition - transform.position).ResetAxis(EAxis.Y).normalized);
                    transform.position = _agent.nextPosition;
                }
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
            var newState = new PlayerMoveState();
            if (state is not PlayerMoveState && state.CanChangeState(newState))
            {
                state.ChangeState(newState);
            }

            if (state is PlayerMoveState)
            {
                _agent.SetDestination(position);
            }
        }

        private void OnPositionReached_UpdateState()
        {
            var newState = new PlayerIdleState();
            if (state.CanChangeState(newState))
            {
                state.ChangeState(newState);
            }
        }
    }
}
