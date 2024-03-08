using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project
{
    public class AttackController : MonoBehaviour
    {
        private PCPlayerRefs _playerRefs;
        private Camera _camera;
        [ShowInInspector, ReadOnly] private readonly Timer _attackCooldown = new Timer();
        [ShowInInspector, ReadOnly] private readonly Timer _castingTimer = new Timer();
        
        
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
            
            if (Utilities.GetMouseWorldHit(_camera, Constants.LayersMask.Entity, out RaycastHit hitInfo))
            {
                if (hitInfo.transform.TryGetComponent(out IDamageable damageable))
                {
                    Debug.Log("Attack request");

                    if (damageable.CanDamage(_playerRefs.TeamIndex))
                    {
                        if (_playerRefs.StateMachine.CanChangeStateTo(_playerRefs.StateMachine.castingState))
                        {
                            StartCast(damageable);
                        }
                        else
                        {
                            StopCast();
                        }
                    }
                }
            }
        }

        private void StartCast(IDamageable damageable)
        {
            StopCast();
            _playerRefs.StateMachine.ChangeState(_playerRefs.StateMachine.castingState);
            _castingTimer.StartTimerWithCallback(this, 0.5f, () => Hit(damageable));
        }

        private void StopCast()
        {
            _castingTimer.StopTimer();
        }


        private void Hit(IDamageable damageable)
        {
            damageable.Damage(20);
            _attackCooldown.StartSimpleTimerScaled(this, 5);
            _playerRefs.StateMachine.ChangeState(_playerRefs.StateMachine.idleState);
        }

        private bool CanAttack()
        {
            return !_castingTimer.isTimerRunning && !_attackCooldown.isTimerRunning;
        }
        
    }
}
