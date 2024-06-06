using System;
using Project.Spells;
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
        public static event Action<UserInstance> OnSpawned;
        public static event Action<UserInstance> OnLocalSpawned;
        public static event Action<UserInstance> OnDespawned;
        public static event Action<UserInstance> OnLocalDespawned;
        public static event Action<UserInstance> OnNameChanged;

        public PlayerRefs LinkedPlayer { get; private set; }
        public event Action<PlayerRefs> OnPlayerLinked;
        
        //NetVars
        [ShowInInspector] private GRPC_NetworkVariable<int> _networkClientId = new("ClientId", value: int.MaxValue);
        [ShowInInspector] private GRPC_NetworkVariable<FixedString64Bytes> _networkScene = new GRPC_NetworkVariable<FixedString64Bytes>("Scene");
        [ShowInInspector] private GRPC_NetworkVariable<FixedString64Bytes> _networkPlayerName = new("Name", value: "UnknowName");
        [ShowInInspector] public GRPC_NetworkVariable<int> _networkTeam = new("Team", value: -1);
        [ShowInInspector] private GRPC_NetworkVariable<bool> _networkIsMobile = new("IsMobile");
        [ShowInInspector] public GRPC_NetworkVariable<bool> _networkIsReady { get; private set; } = new("IsReady");
        [ShowInInspector] private GRPC_NetworkVariable<int> _networkCharacterId = new("CharacterId");
        
        [ShowInInspector] public GRPC_NetworkVariable<int> WinCount { get; private set; } = new("WinCount");
        
        private GRPC_NetworkVariable<int>[] _mobileSpells = new GRPC_NetworkVariable<int>[SpellData.CharacterSpellsCount];
        private GRPC_NetworkVariable<int> _ms1 = new("MobileSpell_0"), _ms2 = new("MobileSpell_1"), _ms3 = new("MobileSpell_2"), _ms4 = new("MobileSpell_3");
        
        public int ClientId => _networkClientId.Value;
        
        public string PlayerName => _networkPlayerName.Value.ToString();
        public int Team => _networkTeam.Value;
        public bool IsMobile => _networkIsMobile.Value;
        public bool IsReady => _networkIsReady.Value;
        public int CharacterId => _networkCharacterId.Value;

        private void Start()
        {
            _networkPlayerName.OnValueChanged += OnPlayerNameChanged_NotifyAll;
        }

        

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            _networkPlayerName.OnValueChanged -= OnPlayerNameChanged_NotifyAll;
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log("[UserInstance] Start spawn", gameObject);

            InitializeNetworkVariables();

            if (IsClient && !IsHost)
            {
                // OnValueChanged is not called for network object that were already spawned before joining
                // We need to call manually
                if(_networkClientId.Value != int.MaxValue) OnClientIdChanged(0, _networkClientId.Value);
                if(_networkTeam.Value != -1) OnTeamChanged(-1, _networkTeam.Value);
                
                _networkClientId.OnValueChanged += OnClientIdChanged;
                _networkTeam.OnValueChanged += OnTeamChanged;
            }

            if (IsServer)
            {
                string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                SrvSetScene(currentSceneName);
            }
            
            if (IsOwner && !GetComponent<GRPC_NetworkObjectSyncer>().IsOwnedByUnrealClient)
            {
                Me = this;
                SetNameServerRpc(PlayerData.playerName);  
                OnLocalSpawned?.Invoke(this);
            }

            OnSpawned?.Invoke(this);
            Debug.Log("[UserInstance] End spawn", gameObject);
        }
        
        public override void OnNetworkDespawn()
        {
            Debug.Log("[UserInstance] Start despawn", gameObject);
            ResetNetworkVariables();

            if (IsClient)
            {
                if(UserInstanceManager.instance) UserInstanceManager.instance.ClientUnregisterUserInstance(this);
                _networkClientId.OnValueChanged -= OnClientIdChanged;
                _networkTeam.OnValueChanged -= OnTeamChanged;
            }
            
            if (IsOwner && !GetComponent<GRPC_NetworkObjectSyncer>().IsOwnedByUnrealClient)
            {
                Me = null;
                OnLocalDespawned?.Invoke(this);
            }
            
            OnDespawned?.Invoke(this);
            Debug.Log("[UserInstance] End despawn", gameObject);
        }

        private void CreateNetVarInstance()
        {
            string prefix = "MobileSpell_";
            
            _ms1 = new GRPC_NetworkVariable<int>(prefix + 0);
            _ms2 = new GRPC_NetworkVariable<int>(prefix + 1);
            _ms3 = new GRPC_NetworkVariable<int>(prefix + 2);
            _ms4 = new GRPC_NetworkVariable<int>(prefix + 3);
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
            WinCount.Initialize();

            _mobileSpells[0] = _ms1;
            _mobileSpells[1] = _ms2;
            _mobileSpells[2] = _ms3;
            _mobileSpells[3] = _ms4;
            
            for (var i = 0; i < _mobileSpells.Length; i++)
            {
                _mobileSpells[i].Initialize();
            }
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
            WinCount.Reset();
            
            for (var i = 0; i < _mobileSpells.Length; i++)
            {
                _mobileSpells[i].Reset();
            }
        }
        
        public void LinkPlayer(PlayerRefs refs)
        {
            Debug.Log($"LinkPlayer for UserInstance {_networkClientId.Value}");
            LinkedPlayer = refs;
            
            OnPlayerLinked?.Invoke(refs);
        }

        public void UnlinkPlayer()
        {
            Debug.Log($"UnlinkPlayer for UserInstance {_networkClientId.Value}");
            LinkedPlayer = null;
            
            // Really useful ?
            OnPlayerLinked?.Invoke(null);
        }
        
        private void OnClientIdChanged(int oldValue, int newValue)
        {
            // Should only happen once when user instance is spawned
            
            Utilities.StartWaitUntilAndDoAction(this, UserInstanceManager.IsInstanceAlive, () =>
            {
                UserInstanceManager.instance.ClientRegisterUserInstance(this);
            });
        }
        
        private void OnTeamChanged(int oldValue, int newValue)
        {            
            Utilities.StartWaitUntilAndDoAction(this, TeamManager.IsInstanceAlive, () =>
            {
                TeamManager.instance.ClientOnTeamChanged(this, oldValue, newValue);
            });
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

        public int GetMobileSpell(int slotId)
        {
            if (slotId < 0 || slotId >= _mobileSpells.Length)
            {
                Debug.LogError($"Can't get mobile spell. Given index {slotId} is out of range");
                return 0;
            }
            
            return _mobileSpells[slotId].Value;
        }
        
        #endregion
        
        
        //Setters
        [Server]
        [Button]
        public void SrvSetClientId(int clientId)
        {
            _networkClientId.Value = clientId;
        }
        
        [Server]
        [Button]
        public void SrvSetScene(string sceneName)
        {
            _networkScene.Value = sceneName;
        }
        
        [Server]
        [Button]
        public void SrvSetName(string playerName)
        {
            _networkPlayerName.Value = playerName;
        }
        
        [ServerRpc]
        public void SetNameServerRpc(string playerName)
        {
            SrvSetName(playerName);
        }
        
        [Server]
        [Button]
        public void SrvSetTeam(int playerTeam)
        {
            _networkTeam.Value = playerTeam;
        }

        [Server]
        [Button]
        public void SrvSetIsMobile(bool isMobile)
        {
            _networkIsMobile.Value = isMobile;
        }
        
        [Server]
        [Button]
        public void SrvSetIsReady(bool isReady)
        {
            _networkIsReady.Value = isReady;
        }

        [Server]
        [Button]
        public void SrvSetCharacter(int characterId)
        {
            _networkCharacterId.Value = characterId;
        }

        [Server]
        [Button]
        public void SrvSetMobileSpell(int index, int spellId)
        {
            if (index < 0 || index >= _mobileSpells.Length)
            {
                Debug.LogError($"Can't set mobile spell. Given index {index} is out of range");
                return;
            }
            
            Debug.Log("SetMobileSpell " + index + " : " + spellId + " for " + _networkClientId.Value + " : " + _networkPlayerName.Value);
            
            _mobileSpells[index].Value = spellId;
        }
        
        private void OnPlayerNameChanged_NotifyAll(FixedString64Bytes previousvalue, FixedString64Bytes newvalue)
        {
            OnNameChanged?.Invoke(this);
        }
        
        #if UNITY_EDITOR
        [ExecuteOnReload]
        private static void StaticClear()
        {
            OnSpawned = null;
        }
        #endif
    }
}