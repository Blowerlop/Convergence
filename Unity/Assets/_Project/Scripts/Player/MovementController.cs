using System;
using Sirenix.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project
{
    public class MovementController : NetworkBehaviour
    {
        [SerializeField] private Transform _character;
        
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _groundLayerMask;

        [SerializeField] private float _lerpTime;
        
        private readonly GRPC_NetworkVariable<NetworkVector3Simplified> _position = new GRPC_NetworkVariable<NetworkVector3Simplified>("Position");
        // private readonly GRPC_NetworkVariable<Quaternion> _rotation = new GRPC_NetworkVariable<Quaternion>("Rotation");
        private readonly GRPC_NetworkVariable<Vector3> _rotation = new GRPC_NetworkVariable<Vector3>("Rotation");

        private void Start()
        {
            _camera = Camera.main;
        }


        private void OnEnable()
        {
            InputManager.instance.onMouseButton0.started += TryGoTo;
        }
        
        private void OnDisable()
        {
            InputManager.instance.onMouseButton0.started -= TryGoTo;
        }


        private void TryGoTo(InputAction.CallbackContext _)
        {
            if (Utilities.GetMouseWorldPosition(_camera, _groundLayerMask, out Vector3 position))
            {
                GoTo(position);
            }
        }

        private void GoTo(Vector3 position)
        {
            StartCoroutine(Utilities.LerpInTimeCoroutine(_lerpTime, _character.position, position, value =>
            {
                _character.position = value;
                // _position.Value = value;
                _position.Value = new NetworkVector3Simplified(value);
            }));
            StartCoroutine(Utilities.LerpInTimeCoroutine(_lerpTime, _character.rotation, Quaternion.LookRotation(position - _character.position), value =>
            {
                _character.rotation = value;
                // _rotation.Value = value.eulerAngles;
            }));
        }
    }
}
