using System;
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
        [SerializeField, OnValueChanged(nameof(ToggleCameraLock))] private bool _lockCamera = true;
        [SerializeField] private LayerMask _groundLayerMask;
        
        [Title("References")]
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private Transform _player;

        private Vector3 _forward;
        private Vector3 _right;
        private Vector3 _offset;

        
        private void Start()
        {
            _forward = (_playerCamera.transform.forward + _playerCamera.transform.up).ResetAxis(EAxis.Y).normalized;
            _right = _playerCamera.transform.right;
            
            if (Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out RaycastHit hit, Mathf.Infinity, _groundLayerMask))
            {
                _offset = _playerCamera.transform.position - hit.point;
                _offset.y = 0.0f;
            }
            else Debug.LogError("Unable to calculate offset");
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Application.isFocused == false) return;
#endif

            if (_lockCamera) return;
            
            ComputeMovement();
        }

        private void LateUpdate()
        {
            if (_lockCamera) CenterCameraOnPlayer();
        }

        private void OnEnable()
        {
            InputManager.instance.onCenterCamera.performed += onCenterCameraRequest_CenterCameraOnPlayer;
            InputManager.instance.onLockCamera.performed += OnCameraLockRequest_ToggleCameraLock;
        }

        private void OnDisable()
        {
            if (InputManager.isBeingDestroyed == false)
            {
                InputManager.instance.onCenterCamera.performed -= onCenterCameraRequest_CenterCameraOnPlayer;
                InputManager.instance.onLockCamera.performed -= OnCameraLockRequest_ToggleCameraLock;
            }
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

        private void OnCameraLockRequest_ToggleCameraLock(InputAction.CallbackContext _)
        {
            ToggleCameraLock();
        }

        private void ToggleCameraLock()
        {
            _lockCamera = !_lockCamera;
            Debug.Log("Camera lock : " + _lockCamera);
        }

        private void onCenterCameraRequest_CenterCameraOnPlayer(InputAction.CallbackContext _)
        {
            CenterCameraOnPlayer();
        }

        [Button]
        private void CenterCameraOnPlayer()
        {
            _playerCamera.transform.position = new Vector3(_player.transform.position.x, _playerCamera.transform.position.y, _player.transform.position.z) + _offset;
        }
    }
}
