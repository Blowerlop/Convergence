using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Netcode;

namespace Project
{
    /// <summary>
    /// Stores client's specific infos.
    /// </summary>
    public class UserInstance : NetworkBehaviour
    {
        /// <summary>
        /// Reference to the local client's UserInstance.
        /// </summary>
        [ClearOnReload] public static UserInstance Me;
        
        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;

            Me = this;
        }
        
        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;

            Me = null;
        }
        
        private void Start()
        {
            if(!IsServer && !IsHost) return;
            
            InitializeNetworkVariables();
        }

        private void InitializeNetworkVariables()
        {
            _name.Initialize();
            _team.Initialize();
        }
        
        //NetVars
        [ShowInInspector] private readonly GRPC_NetworkVariable<FixedString64Bytes> _name = new("Name");
        [ShowInInspector] private readonly GRPC_NetworkVariable<int> _team = new("Team");
        
        //Getters
        public string Name => _name.Value.ToString();
        public int Team => _team.Value;
        
        //Setters
        [Button]
        public void SetName(string n)
        {
            if (!IsServer && !IsHost) return;
            
            _name.Value = n;
        }
        
        [Button]
        public void SetTeam(int t)
        {
            if (!IsServer && !IsHost) return;
            
            _team.Value = t;
        }
    }
}