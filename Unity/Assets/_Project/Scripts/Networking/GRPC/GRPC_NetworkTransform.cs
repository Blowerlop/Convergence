using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Project
{
    
    [DefaultExecutionOrder(100000)] // this is needed to catch the update time after the transform was updated by user scripts
    public class GRPC_NetworkTransform : NetworkTransform
    {
        #region Variables
        private readonly GRPC_NetworkVariable<NetworkVector3Simplified> _position = new GRPC_NetworkVariable<NetworkVector3Simplified>("Position");
        private readonly GRPC_NetworkVariable<NetworkVector3Simplified> _rotation = new GRPC_NetworkVariable<NetworkVector3Simplified>("Rotation");
        private readonly GRPC_NetworkVariable<NetworkVector3Simplified> _scale = new GRPC_NetworkVariable<NetworkVector3Simplified>("Scale");
        #endregion

        
        #region Updates
        
        private void Start()
        {
            InitializeNetworkVariables();
        }

        #endregion


        #region Methods
        
        private void InitializeNetworkVariables()
        {
            _position.Initialize();
            _rotation.Initialize();
            _scale.Initialize();
        }

        protected override void OnNetworkTransformStateUpdated(ref NetworkTransformState oldState, ref NetworkTransformState newState)
        {
            if (IsOwner == false) return;
            
            if (newState.HasPositionChange)
            {
                UpdatePositionOnGRPCServerRpc(newState.GetPosition());
            }

            if (newState.HasRotAngleChange)
            {
                UpdateRotationOnGRPCServerRpc(newState.GetRotation().eulerAngles);
            }

            if (newState.HasScaleChange)
            {
                UpdateScaleOnGRPCServerRpc(newState.GetScale());
            }
        }
        
        [ServerRpc]
        private void UpdatePositionOnGRPCServerRpc(Vector3 newPosition)
        {
            _position.Value = new NetworkVector3Simplified(newPosition);
        }
        
        [ServerRpc]
        private void UpdateRotationOnGRPCServerRpc(Vector3 newRotation)
        {
            _rotation.Value = new NetworkVector3Simplified(newRotation);
        }
        
        [ServerRpc]
        private void UpdateScaleOnGRPCServerRpc(Vector3 newScale)
        {
            _scale.Value = new NetworkVector3Simplified(newScale);
        }
        #endregion
        
    }
}
