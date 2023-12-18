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
        [ShowInInspector] public GRPC_NetworkVariable<bool> _networkIsReady { get; private set; } = new("IsReady");
        [ShowInInspector] private NetworkVariable<int> _networkCharacterId = new();
        
        public string PlayerName => _networkPlayerName.Value.ToString();
        public int Team => _networkTeam.Value;
        public bool IsMobile => _networkIsMobile.Value;
        public bool IsReady => _networkIsReady.Value;
        public int CharacterId => _networkCharacterId.Value;

        
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

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            ResetNetworkVariables();
        }

        private void InitializeNetworkVariables()
        {
            _networkPlayerName.Initialize();
            _networkTeam.Initialize();
            _networkIsMobile.Initialize();
        }

        private void ResetNetworkVariables()
        {
            _networkPlayerName.Reset();
            _networkTeam.Reset();
            _networkIsMobile.Reset();
        }
        
        
        //Setters
        public void SetName(string playerName)
        {
            _networkPlayerName.Value = playerName;
        }
        
        public void SetTeam(int playerTeam)
        {
            _networkTeam.Value = playerTeam;
        }

        public void SetIsMobile(bool isMobile)
        {
            _networkIsMobile.Value = isMobile;
        }
        
        public void SetIsReady(bool isReady)
        {
            _networkIsReady.Value = isReady;
        }
        
        public void SetCharacter(int characterId)
        {
            _networkCharacterId.Value = characterId;
        }
    }
}