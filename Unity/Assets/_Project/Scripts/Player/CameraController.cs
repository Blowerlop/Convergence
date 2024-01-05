using System;
using Project.Extensions;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Project
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private float _speed = 1.0f;
        [SerializeField] private bool _lerp;
        [SerializeField] private float _lerpSpeed = 1.0f;

        private Vector3 _forward;
        private Vector3 _right;

        private void Start()
        {
            _forward = (_playerCamera.transform.forward + _playerCamera.transform.up).RemoveAxis(EAxis.Y).normalized;
            _right = _playerCamera.transform.right;
        }

        private void Update()
        {
            Vector2 movementInput = InputManager.instance.move;
            if (movementInput == Vector2.zero) return;

            Vector3 rawVelocity = _right * movementInput.x + _forward * movementInput.y;
            Vector3 velocity = rawVelocity.normalized * _speed * Time.deltaTime;
            
            _playerCamera.transform.position += velocity;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(_playerCamera.transform.position, _forward);
            Gizmos.DrawRay(_playerCamera.transform.position, _right);
        }
    }
}
