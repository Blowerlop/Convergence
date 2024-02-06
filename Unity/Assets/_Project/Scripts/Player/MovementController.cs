using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Project
{
    public class MovementController : NetworkBehaviour
    {
        
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _groundLayerMask;
        private NavMeshAgent _agent;
        public PlayerState state;

        private Coroutine _movementLerpCoroutine;
        private Coroutine _rotationLerpCoroutine;
        
        public static event Action OnPositionReached;

        private void Start()
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
                state = new PlayerIdleState();
                state.StartState(this);
                OnPositionReached += PositionReached;
            }
            else
                GetComponent<NavMeshAgent>().enabled = false;
        }

        private void OnEnable()
        {
            InputManager.instance.onMouseButton0.started += TryGoTo;
        }
        
        private void OnDisable()
        {
            InputManager.instance.onMouseButton0.started -= TryGoTo;
        }

        private void OnDestroy()
        {
            OnPositionReached -= PositionReached;
        }
        
        private void Update()
        {
            if (state is PlayerMoveState && Math.Abs(_agent.remainingDistance - _agent.stoppingDistance) < 0.1f)
            {
                OnPositionReached?.Invoke();
            }
        }

        private void TryGoTo(InputAction.CallbackContext _)
        {
            if (Utilities.GetMouseWorldPosition(_camera, _groundLayerMask, out Vector3 position))
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

        private void PositionReached()
        {
            var newState = new PlayerIdleState();
            if (state.CanChangeState(newState))
            {
                state.ChangeState(newState);
            }
        }
    }
}
