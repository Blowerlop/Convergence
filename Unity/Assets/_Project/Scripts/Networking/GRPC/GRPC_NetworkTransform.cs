using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Project
{
    
    [DefaultExecutionOrder(100000)] // This is needed to catch the update time after the transform was updated by user scripts
    public class GRPC_NetworkTransform : NetworkTransform
    {
        #region Variables
        private readonly GRPC_NetworkVariable<NetworkVector3Simplified> _position = new GRPC_NetworkVariable<NetworkVector3Simplified>("Position");
        private readonly GRPC_NetworkVariable<NetworkVector3Simplified> _rotation = new GRPC_NetworkVariable<NetworkVector3Simplified>("Rotation");
        private readonly GRPC_NetworkVariable<NetworkVector3Simplified> _scale = new GRPC_NetworkVariable<NetworkVector3Simplified>("Scale");
        #endregion

        
        #region Updates

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            InitializeNetworkVariables();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            ResetNetworkVariables();
        }

        #endregion


        #region Methods
        
        private void InitializeNetworkVariables()
        {
            if (SyncPositionX || SyncPositionY || SyncPositionZ)
            {
                _position.Initialize();
            }
            
            if (SyncRotAngleX || SyncRotAngleY || SyncRotAngleZ)
            {
                _rotation.Initialize();
            }
            
            if (SyncScaleX || SyncScaleY || SyncScaleZ)
            {
                _scale.Initialize();
            }
        }
        
        private void ResetNetworkVariables()
        {
            if (SyncPositionX || SyncPositionY || SyncPositionZ)
            {
                _position.Reset();
            }
            
            if (SyncRotAngleX || SyncRotAngleY || SyncRotAngleZ)
            {
                _rotation.Reset();
            }
            
            if (SyncScaleX || SyncScaleY || SyncScaleZ)
            {
                _scale.Reset();
            }
        }

        protected override void OnNetworkTransformStateUpdated(ref NetworkTransformState oldState, ref NetworkTransformState newState)
        {
            // Cant user server because transform update on server side don't call any event/method
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
