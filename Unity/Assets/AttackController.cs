using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project
{
    public class AttackController : MonoBehaviour
    {
        private PCPlayerRefs _playerRefs;
        private Camera _camera;
        [ShowInInspector, ReadOnly] private readonly Timer _attackTime = new Timer();
        
        
        private void Awake()
        {
            _camera = Camera.main;
            _playerRefs = GetComponentInParent<PCPlayerRefs>();
        }

        private void OnEnable()
        {
            InputManager.instance.onMouseButton1.performed += OnAttackRequest;
        }
        
        private void OnDisable()
        {
            InputManager.instance.onMouseButton1.performed -= OnAttackRequest;
        }
        
        
        private void OnAttackRequest(InputAction.CallbackContext _)
        {
            if (CanAttack() == false) return;
            if (!Utilities.GetMouseWorldHit(_camera, Constants.LayersMask.Entity, out RaycastHit hitInfo)) return;
            if (IsInRange(hitInfo)) return;
            if (IsDamageable(hitInfo, out IDamageable damageable)) return;

            Debug.Log("Attack request");

            if (!damageable.CanDamage(_playerRefs.TeamIndex)) return;
            if (_playerRefs.StateMachine.CanChangeStateTo(_playerRefs.StateMachine.castingState))
            {
                StartCast(damageable);
            }
            else
            {
                StopCast();
            }
        }

        private static bool IsDamageable(RaycastHit hitInfo, out IDamageable damageable)
        {
            return !hitInfo.transform.TryGetComponent(out damageable);
        }

        private bool IsInRange(RaycastHit hitInfo)
        {
            return (hitInfo.transform.position - _playerRefs.PlayerTransform.position).sqrMagnitude > _playerRefs.Entity.Stats.attackRange.Value * _playerRefs.Entity.Stats.attackRange.Value;
        }

        private void StartCast(IDamageable damageable)
        {
            StopCast();
            _playerRefs.StateMachine.ChangeState(_playerRefs.StateMachine.castingState);
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
