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

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
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
        
        protected override void OnAuthorityPushTransformState(ref NetworkTransformState networkTransformState)
        {
            base.OnAuthorityPushTransformState(ref networkTransformState);
            
            if (networkTransformState.HasPositionChange)
            {
                UpdatePositionOnGrpcServer(networkTransformState.GetPosition());
            }

            if (networkTransformState.HasRotAngleChange)
            {
                UpdateRotationOnGrpcServer(networkTransformState.GetRotation().eulerAngles);
            }

            if (networkTransformState.HasScaleChange)
            {
                UpdateScaleOnGrpcServer(networkTransformState.GetScale());
            }
        }

        private void UpdatePositionOnGrpcServer(Vector3 newPosition)
        {
            if (IsServer) UpdatePositionOnGrpcServer_Srv(newPosition);
            else UpdatePositionOnGrpcServer_ServerRpc(newPosition);
        }
        
        [Server]
        private void UpdatePositionOnGrpcServer_Srv(Vector3 newPosition)
        {
            _position.Value = new NetworkVector3Simplified(newPosition);
        }

        [ServerRpc]
        private void UpdatePositionOnGrpcServer_ServerRpc(Vector3 newPosition)
        {
            _position.Value = new NetworkVector3Simplified(newPosition);
        }
        
        private void UpdateRotationOnGrpcServer(Vector3 newRotation)
        {
            if (IsServer) UpdateRotationOnGrpcServer_Srv(newRotation);
            else UpdateRotationOnGrpcServer_ServerRpc(newRotation);
        }
        
        [Server]
        private void UpdateRotationOnGrpcServer_Srv(Vector3 newRotation)
        {
            _rotation.Value = new NetworkVector3Simplified(newRotation);
        }

        [ServerRpc]
        private void UpdateRotationOnGrpcServer_ServerRpc(Vector3 newRotation)
        {
            _rotation.Value = new NetworkVector3Simplified(newRotation);
        }
        
        private void UpdateScaleOnGrpcServer(Vector3 newScale)
        {
            if (IsServer) UpdateScaleOnGrpcServer_Srv(newScale);
            else UpdateScaleOnGrpcServer_ServerRpc(newScale);
        }
        
        [Server]
        private void UpdateScaleOnGrpcServer_Srv(Vector3 newScale)
        {
            _scale.Value = new NetworkVector3Simplified(newScale);
        }

        [ServerRpc]
        private void UpdateScaleOnGrpcServer_ServerRpc(Vector3 newScale)
        {
            _scale.Value = new NetworkVector3Simplified(newScale);
        }
        #endregion
    }
}
