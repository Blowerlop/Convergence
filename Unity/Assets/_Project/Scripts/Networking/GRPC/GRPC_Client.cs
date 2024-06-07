using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Netcode;

namespace Project
{
    public class GRPC_Client : NetworkBehaviour
    {
        [ShowInInspector] private readonly GRPC_NetworkVariable<FixedString64Bytes> _name = new GRPC_NetworkVariable<FixedString64Bytes>("Name");
        [ShowInInspector] private readonly GRPC_NetworkVariable<int> _health = new GRPC_NetworkVariable<int>("Health");
        [ShowInInspector] private readonly GRPC_NetworkVariable<FixedString64Bytes> _currentAnimation = new GRPC_NetworkVariable<FixedString64Bytes>("CurrentAnimation");
        [ShowInInspector] private readonly GRPC_NetworkVariable<int> _team = new GRPC_NetworkVariable<int>("Team");
        [ShowInInspector] private readonly GRPC_NetworkVariable<int> _mesh = new GRPC_NetworkVariable<int>("Mesh");


        private void Start()
        {
            InitializeNetworkVariables();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            ResetNetworkVariables();
        }

        private void InitializeNetworkVariables()
        {
            _name.Initialize();
            _health.Initialize();
            _team.Initialize();
            _mesh.Initialize();
            _currentAnimation.Initialize();
        }
        
        private void ResetNetworkVariables()
        {
            _name.Reset();
            _health.Reset();
            _team.Reset();
            _mesh.Reset();
            _currentAnimation.Reset();
        }

        [Button]
        private void UpdateName(string newName)
        {
            _name.Value = new FixedString64Bytes(newName);
        }
        
        [Button]
        private void UpdateHealth(int amount)
        {
            _health.Value = amount;
        }
        
        [Button]
        private void UpdateAnimation(string animation)
        {
            _currentAnimation.Value = new FixedString64Bytes(animation);
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