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
        [ShowInInspector] private readonly GRPC_NetworkVariable<FixedString64Bytes> _name = new("Name", value: "UnknowName");
        [ShowInInspector] private readonly GRPC_NetworkVariable<int> _team = new("Team", value: -1);
        
        //Getters
        public string PlayerName => _name.Value.ToString();
        public int Team => _team.Value;
        
        //Setters
        [ServerRpc, Button]
        public void SetNameServerRpc(string playerName)
        {
            SetNameClientRpc(playerName);
        }

        [ClientRpc]
        private void SetNameClientRpc(string playerName)
        {
            SetName(playerName);
        }
        
        private void SetName(string playerName)
        {
            _name.Value = playerName;
        }
        
        [ServerRpc, Button]
        public void SetTeamServerRpc(int playerTeam)
        {
            SetTeamClientRpc(playerTeam);
        }

        [ClientRpc]
        private void SetTeamClientRpc(int playerTeam)
        {
            SetTeam(playerTeam);
        }
        
        public void SetTeam(int playerTeam)
        {
            _team.Value = playerTeam;
        }
    }
}