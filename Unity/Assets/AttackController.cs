using Project.Extensions;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project
{
    public class AttackController : NetworkBehaviour
    {
        private PCPlayerRefs _playerRefs;
        private Camera _camera;
        [ShowInInspector, ReadOnly] private readonly Timer _attackTime = new Timer();


        private void Awake()
        {
            _playerRefs = GetComponentInParent<PCPlayerRefs>();
            _camera = Camera.main;

        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                InputManager.instance.onMouseButton1.performed += OnMouseButton1_AttackRequest;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsOwner)
            {
                InputManager.instance.onMouseButton1.performed -= OnMouseButton1_AttackRequest;
            }
        }
        
        private void OnMouseButton1_AttackRequest(InputAction.CallbackContext _)
        {
            if (!Utilities.GetMouseWorldHit(_camera, Constants.LayersMask.Entity, out RaycastHit target)) return;
            if (!IsDamageable(target.transform, out IDamageable damageable)) return;
            if (!damageable.CanDamage(_playerRefs.TeamIndex)) return;

            TryToAttackServerRpc(target.transform.GetComponentInParent<NetworkObject>());
        }

        [ServerRpc]
        private void TryToAttackServerRpc(NetworkObjectReference networkObjectReference)
        {
            if (CanAttack() == false) return;

            NetworkObject targetNetObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectReference.NetworkObjectId];
            
            if (IsInRange(targetNetObject.transform.position)) return;
            

            Debug.Log("Attack request");

            IDamageable damageable = targetNetObject.GetComponentInChildren<IDamageable>();
            
            if (_playerRefs.StateMachine.CanChangeStateTo(_playerRefs.StateMachine.castingState))
            {
                StartCast(targetNetObject.transform.position, damageable);
            }
            else
            {
                StopCast();
            }
        }

        private bool IsDamageable(Transform target, out IDamageable damageable)
        {
            return target.TryGetComponent(out damageable);
        }

        private bool IsInRange(Vector3 targetPosition)
        {
            return (targetPosition - _playerRefs.PlayerTransform.position).sqrMagnitude > _playerRefs.Entity.Stats.attackRange.Value * _playerRefs.Entity.Stats.attackRange.Value;
        }

        private void StartCast(Vector3 targetPosition, IDamageable damageable)
        {
            StopCast();
            _playerRefs.StateMachine.ChangeState(_playerRefs.StateMachine.castingState);
            _playerRefs.PlayerTransform.rotation = Quaternion.LookRotation((targetPosition - _playerRefs.PlayerTransform.position).ResetAxis(EAxis.Y).normalized);
            _attackTime.StartTimerWithCallback(this, _playerRefs.Entity.Stats.attackSpeed.Value, () => Hit(damageable));
        }

        private void StopCast()
        {
            _attackTime.StopTimer();
        }


        private void Hit(IDamageable damageable)
        {
            damageable.Damage(_playerRefs.Entity.Stats.attackDamage.Value);
            _playerRefs.StateMachine.ChangeState(_playerRefs.StateMachine.idleState);
        }

        private bool CanAttack()
        {
            return !_attackTime.isTimerRunning;
        }
        
    }
}
