using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

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
        
        //NetVars
        [ShowInInspector] private GRPC_NetworkVariable<FixedString64Bytes> _networkPlayerName = new("Name", value: "UnknowName");
        [ShowInInspector] private GRPC_NetworkVariable<int> _networkTeam = new("Team", value: -1);
        [ShowInInspector] private GRPC_NetworkVariable<bool> _networkIsMobile = new("IsMobile");
        
        public string PlayerName => _networkPlayerName.Value.ToString();
        public int Team => _networkTeam.Value;

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
            _networkPlayerName.Initialize();
            _networkTeam.Initialize();
            _networkIsMobile.Initialize();
        }

        
        
        
        
        
        //Setters
        [ServerRpc, Button]
        public void SetNameServerRpc(string playerName)
        {
            SetNameLocal(playerName);
        }
        
        private void SetNameLocal(string playerName)
        {
            _networkPlayerName.Value = playerName;
        }
        
        [ServerRpc, Button]
        public void SetTeamServerRpc(int playerTeam)
        {
            SetTeamLocal(playerTeam);
        }

        private void SetTeamLocal(int playerTeam)
        {
            _networkTeam.Value = playerTeam;
        }

        [ServerRpc, Button]
        public void SetIsMobileServerRpc(bool isMobile)
        {
            IsMobile(isMobile);
        }
        
        private void IsMobile(bool isMobile)
        {
            _networkIsMobile.Value = isMobile;
        }
    }
}