using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Project
{
    
    [DefaultExecutionOrder(100000)] // this is needed to catch the update time after the transform was updated by user scripts
    public class GRPC_NetworkTransform : NetworkTransform
    {
        private readonly GRPC_NetworkVariable<NetworkVector3Simplified> _position = new GRPC_NetworkVariable<NetworkVector3Simplified>("Position");
        private readonly GRPC_NetworkVariable<NetworkVector3Simplified> _rotation = new GRPC_NetworkVariable<NetworkVector3Simplified>("Position");
        private readonly GRPC_NetworkVariable<NetworkVector3Simplified> _scale = new GRPC_NetworkVariable<NetworkVector3Simplified>("Scale");


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
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
            _position.Value = new NetworkVector3Simplified(newRotation);
        }
        
        [ServerRpc]
        private void UpdateScaleOnGRPCServerRpc(Vector3 newScale)
        {
            _position.Value = new NetworkVector3Simplified(newScale);
        }
        
        // private void Update()
        // {
        //     
        //     if (IsServer)
        //     {
        //         _position.Value = new NetworkVector3Simplified(transform.position);
        //     }
        //     else
        //     {
        //         NetworkVector3Simplified networkVector3Simplified = _position.Value;
        //         transform.position = new Vector3(networkVector3Simplified.x, networkVector3Simplified.y, networkVector3Simplified.z);
        //     }
        // }
    }
}
