using System.Collections;
using Project._Project.Scripts.Player.States;
using Project._Project.Scripts.StateMachine;
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
        private NetworkObject _targetNetworkObject;
        private bool _isAttacking;
        private IDamageable _damageable;


        private void Awake()
        {
            _playerRefs = GetComponentInParent<PCPlayerRefs>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _playerRefs.PlayerMouse.OnMouseClick += OnMouseButton1_AttackRequest;
                InputManager.instance.onCancellation.performed += OnCancellation_StopAttack;
            }

            if (IsServer)
            {
                _playerRefs.StateMachine.OnStateExit += OnStateExit_EndAttack;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsOwner)
            {
                _playerRefs.PlayerMouse.OnMouseClick -= OnMouseButton1_AttackRequest;
                if (InputManager.IsInstanceAlive())
                    InputManager.instance.onCancellation.performed -= OnCancellation_StopAttack;
            }

            if (IsServer)
            {
                _playerRefs.StateMachine.OnStateExit -= OnStateExit_EndAttack;
            }
        }

        private void FixedUpdate()
        {
            if (IsServer && _targetNetworkObject != null)
            {
                if (IsInRange(_targetNetworkObject.transform.position))
                {
                    SrvTryToAttack(_targetNetworkObject);
                }
                else GoToContact(_targetNetworkObject.transform);
            }
        }

        // [Owner]
        private void OnMouseButton1_AttackRequest(RaycastHit hitInfo, int layer)
        {
            if (layer != Constants.Layers.EntityIndex)
            {
                if (_targetNetworkObject != null)
                {
                    _targetNetworkObject = null;
                    RemoveTargetServerRpc();
                }

                return;
            }

            if (!IsDamageable(hitInfo.transform, out IDamageable damageable)) return;
            if (!damageable.CanDamage(_playerRefs.TeamIndex)) return;

            _targetNetworkObject = hitInfo.transform.GetComponentInParent<NetworkObject>();
            if (_targetNetworkObject == null)
            {
                Debug.LogError("Target to attack is not a network object");
                return;
            }

            TryToAttackServerRpc(_targetNetworkObject);
        }

        [ServerRpc]
        private void TryToAttackServerRpc(NetworkObjectReference networkObjectReference)
        {
            SrvTryToAttack(networkObjectReference);
        }

        [Server]
        private void SrvTryToAttack(NetworkObjectReference networkObjectReference)
        {
            networkObjectReference.TryGet(out _targetNetworkObject);
            if (_targetNetworkObject == null)
            {
                Debug.LogError("Not NetworkObject found for the networkObjectReference id " +
                               networkObjectReference.NetworkObjectId);
                return;
            }

            if (IsAttacking())
            {
                if (_targetNetworkObject.NetworkObjectId == networkObjectReference.NetworkObjectId) return;

                // We switch of target
                EndAttack();
            }

            if (IsInRange(_targetNetworkObject.transform.position) == false) return;

            IDamageable damageable = _targetNetworkObject.GetComponentInChildren<IDamageable>();

            if (_playerRefs.StateMachine.CanChangeStateTo(_playerRefs.StateMachine.attackState))
            {
                StartAttack(_targetNetworkObject.transform.position, damageable);
            }
        }

        [Server]
        private void StartAttack(Vector3 targetPosition, IDamageable damageable)
        {
            _isAttacking = true;
            _playerRefs.Animator.runtimeAnimatorController =
                ((PlayerController)_playerRefs.Entity)._attackOverrideController;
            _playerRefs.StateMachine.ChangeState(_playerRefs.StateMachine.attackState);
            _playerRefs.PlayerTransform.rotation =
                Quaternion.LookRotation((targetPosition - _playerRefs.PlayerTransform.position).ResetAxis(EAxis.Y)
                    .normalized);
            _damageable = damageable;
        }

        [Server]
        private IEnumerator EndAttack()
        {
            yield return null;
            
            if (_playerRefs.StateMachine.currentState is AttackState)
            {
                _playerRefs.StateMachine.ChangeState(_playerRefs.StateMachine.idleState);
            }
            
            _isAttacking = false;
        }

        [Server]
        private void Hit(IDamageable damageable)
        {
            damageable.Damage(_playerRefs.Entity.Stats.attackDamage.Value);
            StartCoroutine(EndAttack());
        }

        // Called by animation event
        public void Hit()
        {
            if (IsServer == false) return;

            Hit(_damageable);
        }

        private void OnCancellation_StopAttack(InputAction.CallbackContext _)
        {
            StopAttackServerRpc();
        }

        [ServerRpc]
        private void StopAttackServerRpc()
        {
            _targetNetworkObject = null;
            StartCoroutine(EndAttack());
        }

        [Server]
        private void GoToContact(Transform target)
        {
            _playerRefs.MovementCOntroller.SrvGoTo(target.position);
        }

        [ServerRpc]
        private void RemoveTargetServerRpc()
        {
            _targetNetworkObject = null;
        }

        private bool IsDamageable(Transform target, out IDamageable damageable)
        {
            return target.TryGetComponent(out damageable);
        }

        [Server]
        private bool IsInRange(Vector3 targetPosition)
        {
            return (targetPosition - _playerRefs.PlayerTransform.position).ResetAxis(EAxis.Y).sqrMagnitude <
                   _playerRefs.Entity.Stats.attackRange.Value * _playerRefs.Entity.Stats.attackRange.Value;
        }

        [Server]
        private bool IsAttacking()
        {
            return _isAttacking;
        }

        [Server]
        private void OnStateExit_EndAttack(BaseStateMachine exitState)
        {
            // In case we want to move the moment we're attacking. Cancel the attack and move
            if (exitState is AttackState)
            {
                if (IsAttacking())
                {
                    StartCoroutine(EndAttack());
                }
            }
        }
    }
}
