using System;
using System.Collections;
using Project._Project.Scripts;
using Project._Project.Scripts.Player.States;
using Project._Project.Scripts.StateMachine;
using Project.Effects;
using Project.Extensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project
{
    public class AttackController : NetworkBehaviour
    {
        private PCPlayerRefs _playerRefs;
        private Camera _camera;
        public NetworkObject targetNetworkObject;
        private bool _isAttacking;
        private IDamageable _damageable;
        private bool _isRanged;
        private SOProjectile _projectileData;
        
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

                _playerRefs.Entity.onEntityInit += OnEntityInit;
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
            if (IsServer && targetNetworkObject != null)
            {
                if (IsInRange(targetNetworkObject.transform.position))
                {
                    SrvTryToAttack(targetNetworkObject);
                }
                else GoToContact(targetNetworkObject.transform);
            }
        }

        private void OnEntityInit()
        {
            SOCharacter characterData = (SOCharacter)_playerRefs.Entity.data;
            _isRanged = characterData.attackType == EAttackType.Ranged;
            if (_isRanged) _projectileData = characterData.projectileData;
        }

        // [Owner]
        private void OnMouseButton1_AttackRequest(RaycastHit hitInfo, int layer)
        {
            if (!Gameloop.IsGameRunning) return;
            
            if (layer != Constants.Layers.EntityIndex)
            {
                if (targetNetworkObject != null)
                {
                    targetNetworkObject = null;
                    RemoveTargetServerRpc();
                }

                return;
            }

            if (!IsDamageable(hitInfo.transform, out IDamageable _)) return;

            targetNetworkObject = hitInfo.transform.GetComponentInParent<NetworkObject>();
            if (targetNetworkObject == null)
            {
                Debug.LogError("Target to attack is not a network object");
                return;
            }

            TryToAttackServerRpc(targetNetworkObject);
        }

        [ServerRpc]
        private void TryToAttackServerRpc(NetworkObjectReference networkObjectReference)
        {
            SrvTryToAttack(networkObjectReference);
        }

        [Server]
        private void SrvTryToAttack(NetworkObjectReference networkObjectReference)
        {
            if (!Gameloop.IsGameRunning) return;
            
            networkObjectReference.TryGet(out targetNetworkObject);
            if (targetNetworkObject == null)
            {
                Debug.LogError("Not NetworkObject found for the networkObjectReference id " +
                               networkObjectReference.NetworkObjectId);
                return;
            }

            if (IsAttacking())
            {
                if (targetNetworkObject.NetworkObjectId == networkObjectReference.NetworkObjectId) return;

                // We switch of target
                StartCoroutine(EndAttack());
            }

            if (IsInRange(targetNetworkObject.transform.position) == false) return;

            IDamageable damageable = targetNetworkObject.GetComponentInChildren<IDamageable>();

            if (_playerRefs.StateMachine.CanChangeStateTo<AttackState>())
            {
                StartAttack(targetNetworkObject.transform.position, damageable);
            }
        }

        [Server]
        private void StartAttack(Vector3 targetPosition, IDamageable damageable)
        {
            _isAttacking = true;
            _playerRefs.Animator.SetFloat(Constants.AnimatorsParam.AttackSpeed, _playerRefs.Entity.Stats.Get<AttackSpeedStat>().value);
            _playerRefs.StateMachine.ChangeStateTo<AttackState>();
            _playerRefs.PlayerTransform.rotation =
                Quaternion.LookRotation((targetPosition - _playerRefs.PlayerTransform.position).ResetAxis(EAxis.Y)
                    .normalized);
            _damageable = damageable;
        }

        // Ca ne se passe pas bien dans l'animator si on ne skip pas une frame :)
        [Server]
        private IEnumerator EndAttack()
        {
            yield return null;
            
            if (_playerRefs.StateMachine.currentState is AttackState)
            {
                _playerRefs.StateMachine.ChangeStateTo<IdleState>();
            }

            ResetAttack();
        }

        private void ResetAttack()
        {
            _isAttacking = false;
        }

        [Server]
        public void Hit(IDamageable damageable)
        {
            damageable.Damage(_playerRefs.Entity.Stats.Get<AttackDamageStat>().value);
            
            if(damageable is IEffectable hitEffectable)
            {
                IEffectable ownEffectable = _playerRefs.Entity;
                
                for (int i = ownEffectable.AppliedEffects.Count - 1; i >= 0; i--)
                {
                    var effect = ownEffectable.AppliedEffects[i];
                    if (effect is not NextAutoEffect nextAutoEffect) continue;
                    
                    nextAutoEffect.TryApplyChildEffects(hitEffectable, _playerRefs);
                    nextAutoEffect.KillEffect();
                }
            }
            
            if (_isRanged == false) StartCoroutine(EndAttack());
        }

        // Called by animation event
        public void Hit()
        {
            if (IsServer == false) return;
            
            // We have lost the target during the attack because we wanted to cancel the attack but the animation transition made it to this point
            if (targetNetworkObject == null) return;

            if (_isRanged)
            {
                Projectile projectileInstance = Instantiate(_projectileData.prefab, _playerRefs.ShootTransform.position, _playerRefs.ShootTransform.rotation);
                projectileInstance.Init(this, _projectileData);
                projectileInstance.GetComponent<NetworkObject>().Spawn(true);

                StartCoroutine(EndAttack());
            }
            else
            {
                Hit(_damageable);
            }

        }
        
        private void OnCancellation_StopAttack(InputAction.CallbackContext _)
        {
            StopAttackServerRpc();
        }

        [ServerRpc]
        private void StopAttackServerRpc()
        {
            targetNetworkObject = null;
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
            targetNetworkObject = null;
        }

        public bool IsDamageable(Transform target, out IDamageable damageable)
        {
            return target.TryGetComponent(out damageable) && damageable.CanDamage(_playerRefs.TeamIndex);
        }

        [Server]
        private bool IsInRange(Vector3 targetPosition)
        {
            float attackRange = _playerRefs.Entity.Stats.Get<AttackRangeStat>().value;
            return (targetPosition - _playerRefs.PlayerTransform.position).ResetAxis(EAxis.Y).sqrMagnitude <
                   attackRange * attackRange;
        }

        [Server]
        private bool IsAttacking()
        {
            return _isAttacking;
        }

        [Server]
        private void OnStateExit_EndAttack(BaseStateMachineBehaviour exitState)
        {
            // In case we want to move the moment we're attacking. Cancel the attack and move
            if (exitState is AttackState)
            {
                if (IsAttacking())
                {
                    ResetAttack();
                }
            }
        }

        [Server]
        public void SrvForceReset()
        {
            targetNetworkObject = null;
            ResetAttack();
        }
    }
}
