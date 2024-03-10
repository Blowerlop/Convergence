using System;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project
{
    public class PlayerMouse : NetworkBehaviour
    {
        /// <summary>
        /// Owner authoritative event for when the player clicks the mouse and hits something.
        /// 2nd argument is the layer index of the object hit.
        /// </summary>
        public event Action<RaycastHit, int> OnMouseClick;
        private Camera _camera;
        [SerializeField] private LayerMask _layerMask = Constants.Layers.GroundMask | Constants.Layers.EntityMask;
        
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _camera = Camera.main;
                InputManager.instance.onMouseButton1.performed += OnMouseButton1_FireEvent;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (IsOwner)
            {
                _camera = null;
                if (InputManager.IsInstanceAlive()) InputManager.instance.onMouseButton1.performed -= OnMouseButton1_FireEvent;
            }
        }


        private void OnMouseButton1_FireEvent(InputAction.CallbackContext _)
        {
            if (Utilities.GetMouseWorldHit(_camera, _layerMask, out RaycastHit hitInfo))
            {
                OnMouseClick?.Invoke(hitInfo, hitInfo.collider.gameObject.layer);
            }
        }
    }
}