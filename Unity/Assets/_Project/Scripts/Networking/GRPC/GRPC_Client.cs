using System;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project
{
    public class GRPC_Client : NetworkBehaviour
    {
        private readonly GRPC_NetworkVariable<NetworkString> _name = new GRPC_NetworkVariable<NetworkString>("Name");
        private readonly GRPC_NetworkVariable<int> _health = new GRPC_NetworkVariable<int>("Health");
        // private readonly GRPC_NetworkVariable<Vector3> _position = new GRPC_NetworkVariable<Vector3>("Position");
        // private readonly GRPC_NetworkVariable<Quaternion> _rotation = new GRPC_NetworkVariable<Quaternion>("Rotation");
        private readonly GRPC_NetworkVariable<NetworkString> _currentAnimation = new GRPC_NetworkVariable<NetworkString>("CurrentAnimation");
        private readonly GRPC_NetworkVariable<int> _team = new GRPC_NetworkVariable<int>("Team");
        private readonly GRPC_NetworkVariable<int> _mesh = new GRPC_NetworkVariable<int>("Mesh");
        
        private void OnDisable()
        {
            if (GRPC_Rtt.isBeingDestroyed) return;
        }

        [Button]
        private void UpdateName(string newName)
        {
            _name.Value = new NetworkString(newName);
        }
        
        [Button]
        private void UpdateHealth(int amount)
        {
            _health.Value = amount;
        }
        
        // [Button]
        // private void UpdatePosition(Vector3 position)
        // {
        //     _position.Value = position;
        // }
        
        // [Button]
        // private void UpdateRotation(Vector3 rotation)
        // {
        //     _rotation.Value = Quaternion.Euler(rotation);
        // }
        
        [Button]
        private void UpdateAnimation(string animation)
        {
            _currentAnimation.Value = new NetworkString(animation);
        }
        
        [Button]
        private void UpdateTeam(int team)
        {
            _team.Value = team;
        }
        
        [Button]
        private void UpdateMesh(int mesh)
        {
            _mesh.Value = mesh;
        }
    }
}