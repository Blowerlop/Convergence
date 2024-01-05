using System;
using System.Runtime.CompilerServices;
using Project.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

namespace Project
{
    public static class ScreenBorder
    {
        public static int left => 0;
        public static int right => Screen.width;
        public static int top => Screen.height;
        public static int bottom => 0;
    }
    
    public class CameraController : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField] private float _speed = 40.0f;
        [SerializeField] private bool _canUseKeyboard = true;
        [SerializeField] private bool _canUseMouse = true;
        
        [Title("References")]
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Transform _player;

        private Vector3 _forward;
        private Vector3 _right;

        
        private void Start()
        {
            _forward = (_playerCamera.transform.forward + _playerCamera.transform.up).RemoveAxis(EAxis.Y).normalized;
            _right = _playerCamera.transform.right;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Application.isFocused == false) return;
#endif
            
            ComputeMovement();
        }
        

        private void ComputeMovement()
        {
            Vector2 movementInput = Vector2.zero;
            
            if (_canUseKeyboard) ComputeMovementWithKeyboard(ref movementInput);
            if (_canUseMouse) ComputeMovementWithMouse(ref movementInput);
            
            if (HasRequestedAMovement(movementInput) == false) return;
            
            Vector3 rawVelocity = _right * movementInput.x + _forward * movementInput.y;
            Vector3 velocity = rawVelocity.normalized * (_speed * Time.deltaTime);
            
            _playerCamera.transform.position += velocity;
        }

        private void ComputeMovementWithKeyboard(ref Vector2 movementInput)
        {
            movementInput = InputManager.instance.move;
        }

        private void ComputeMovementWithMouse(ref Vector2 movementInput)
        {
            Vector2 mousePosition = Mouse.current.position.value;

            if (mousePosition.x <= ScreenBorder.left) movementInput.x = -1.0f;
            else if (mousePosition.x >= ScreenBorder.right) movementInput.x = 1.0f;
            
            if (mousePosition.y >= ScreenBorder.top) movementInput.y = 1.0f;
            else if (mousePosition.y <= ScreenBorder.bottom) movementInput.y = -1.0f;
        }
        
        private bool HasRequestedAMovement(Vector2 movementInput)
        {
            return movementInput != Vector2.zero;
        }

        /// <summary>
        /// There is maybe a wayyyy more efficient way to do it but in works for the moment.
        /// </summary>
        [Button]
        private void CenterCameraOnPlayer()
        {
            if (Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out RaycastHit hit))
            {
            }
            else return;

            var result = (_player.transform.position - hit.point).RemoveAxis(EAxis.Y);
            _playerCamera.transform.position += result;
        }


        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(_playerCamera.transform.position, _player.transform.position);
        }
    }
}
