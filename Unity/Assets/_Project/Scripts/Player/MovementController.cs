using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Project
{
    public class MovementController : NetworkBehaviour
    {
        [SerializeField] private Transform _character;
        
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _groundLayerMask;
        private NavMeshAgent _agent;
        public PlayerState state;

        [SerializeField] private float _lerpTime;

        private Coroutine _movementLerpCoroutine;
        private Coroutine _rotationLerpCoroutine;
        
        public static event Action OnPositionReached;

        private void Start()
        {
            _camera = Camera.main;
            if (NetworkManager.IsServer || true)
            {
                _agent = GetComponent<NavMeshAgent>();
                state = new PlayerIdleState();
                state.StartState(this);
                OnPositionReached += PositionReached;
            }
            else
                GetComponent<NavMeshAgent>().enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            enabled = IsOwner;
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
        }

        [ServerRpc]
        private void GoToServerRpc(Vector3 position)
        {
            if (_movementLerpCoroutine != null) StopCoroutine(_movementLerpCoroutine); 
            if (_rotationLerpCoroutine != null) StopCoroutine(_rotationLerpCoroutine); 
            
            _movementLerpCoroutine = StartCoroutine(Utilities.LerpInTimeCoroutine(_lerpTime, _character.position, position, value =>
            {
                _character.position = value;
            }));
            _rotationLerpCoroutine = StartCoroutine(Utilities.LerpInTimeCoroutine(_lerpTime, _character.rotation, Quaternion.LookRotation(position - _character.position), value =>
            {
                _character.rotation = value;
            }));
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
