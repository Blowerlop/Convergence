using System;
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
        
        public PlayerRefs LinkedPlayer { get; private set; }
        public event Action<PlayerRefs> OnPlayerLinked;
        
        //NetVars
        [ShowInInspector] private GRPC_NetworkVariable<int> _networkClientId = new("ClientId", value: int.MaxValue);
        [ShowInInspector] private GRPC_NetworkVariable<FixedString64Bytes> _networkScene = new GRPC_NetworkVariable<FixedString64Bytes>("Scene");
        [ShowInInspector] private GRPC_NetworkVariable<FixedString64Bytes> _networkPlayerName = new("Name", value: "UnknowName");
        [ShowInInspector] private GRPC_NetworkVariable<int> _networkTeam = new("Team", value: -1);
        [ShowInInspector] private GRPC_NetworkVariable<bool> _networkIsMobile = new("IsMobile");
        [ShowInInspector] public GRPC_NetworkVariable<bool> _networkIsReady { get; private set; } = new("IsReady");
        [ShowInInspector] private GRPC_NetworkVariable<int> _networkCharacterId = new("CharacterId");
        
        public int ClientId => _networkClientId.Value;
        
        public string PlayerName => _networkPlayerName.Value.ToString();
        public int Team => _networkTeam.Value;
        public bool IsMobile => _networkIsMobile.Value;
        public bool IsReady => _networkIsReady.Value;
        public int CharacterId => _networkCharacterId.Value;

        
        public override void OnNetworkSpawn()
        {
            InitializeNetworkVariables();

            if (IsClient)
            {
                Debug.LogError("UserInstance OnNetworkSpawn");
                
                // OnValueChanged is not called for network object that were already spawned before joining
                // We need to call manually
                if(_networkClientId.Value != int.MaxValue) OnClientIdChanged(0, _networkClientId.Value);
                if(_networkTeam.Value != -1) OnTeamChanged(-1, _networkTeam.Value);
                
                _networkClientId.OnValueChanged += OnClientIdChanged;
                _networkTeam.OnValueChanged += OnTeamChanged;
            }
            
            if (!IsOwner) return;

            Me = this;
        }
        
        public override void OnNetworkDespawn()
        {
            ResetNetworkVariables();

            if (IsClient)
            {
                if(UserInstanceManager.instance) UserInstanceManager.instance.ClientUnregisterUserInstance(this);
                _networkClientId.OnValueChanged -= OnClientIdChanged;
                _networkTeam.OnValueChanged -= OnTeamChanged;
            }
            
            if (!IsOwner) return;

            Me = null;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            ResetNetworkVariables();
        }

        private void InitializeNetworkVariables()
        {
            _networkClientId.Initialize();
            _networkScene.Initialize();
            _networkPlayerName.Initialize();
            _networkTeam.Initialize();
            _networkIsMobile.Initialize();
            _networkIsReady.Initialize();
            _networkCharacterId.Initialize();
        }

        private void ResetNetworkVariables()
        {
            _networkClientId.Reset();
            _networkScene.Reset();
            _networkPlayerName.Reset();
            _networkTeam.Reset();
            _networkIsMobile.Reset();
            _networkIsReady.Reset();
            _networkCharacterId.Reset();
        }
        
        public void LinkPlayer(PlayerRefs refs)
        {
            Debug.Log($"LinkPlayer for UserInstance {_networkClientId}");
            LinkedPlayer = refs;
            
            OnPlayerLinked?.Invoke(refs);
        }

        public void UnlinkPlayer()
        {
            Debug.Log($"UnlinkPlayer for UserInstance {_networkClientId}");
            LinkedPlayer = null;
            
            // Really useful ?
            OnPlayerLinked?.Invoke(null);
        }
        
        private void OnClientIdChanged(int oldValue, int newValue)
        {
            // Should only happen once when user instance is spawned

            UserInstanceManager.instance.ClientRegisterUserInstance(this);
        }
        
        private void OnTeamChanged(int oldValue, int newValue)
        {            
            TeamManager.instance.ClientOnTeamChanged(this, oldValue, newValue);
        }
        
        #region Getters

        public PlayerPlatform GetPlatform()
        {
            return _networkIsMobile.Value switch
            {
                true => PlayerPlatform.Mobile,
                false => PlayerPlatform.Pc
            };
        }
        
        #endregion
        
        
        //Setters
        public void SetClientId(int clientId)
        {
            if (!IsServer && !IsHost) return;
            
            _networkClientId.Value = clientId;
        }
        
        public void SetScene(string sceneName)
        {
            _networkScene.Value = sceneName;
        }
        
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