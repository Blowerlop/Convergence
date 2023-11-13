using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class UserInstanceManager : NetworkBehaviour
    {
        // I need to put the singleton directly in the class because NGO do NOT support RPCs in generic base classes atm. See the github ticket below
        // https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/2193
        #region Singleton
        private static bool _authorityCheck = true;
        private static NetworkVariableWritePermission _instanceReadPerm = NetworkVariableWritePermission.Server;
        
        private bool _dontDestroyOnLoad = true;
        public static bool isBeingDestroyed { get; private set; }
        
        private static UserInstanceManager _instance = null;
        public static UserInstanceManager instance {
            get
            {
                if (_authorityCheck && CanClientRead() == false)
                {
                    Debug.LogError("Trying to access a NetworkSingleton instance as non Server Client");
                    return null;
                }
                
                if(_instance == null){
                    _instance = FindObjectOfType<UserInstanceManager>();
                    if(_instance == null){
                        GameObject singletonObj = new GameObject();
                        singletonObj.name = typeof(UserInstanceManager).ToString();
                        _instance = singletonObj.AddComponent<UserInstanceManager>();
                    } 
                }
                
                return _instance;
            }
        }
        
        public static bool IsInstanceAlive() => _instance != null;

        private static bool CanClientRead()
        {
            switch (_instanceReadPerm)
            {
                case NetworkVariableWritePermission.Owner:
                    return false;

                default:
                case NetworkVariableWritePermission.Server:
                    return NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId; 
            }
        }
        #endregion
        
        
        [SerializeField, Required, AssetsOnly] private UserInstance _userInstancePrefab;
        [ShowInInspector, ReadOnly] private readonly Dictionary<ulong, UserInstance> _userInstances = new Dictionary<ulong, UserInstance>();
        
        
        private void Awake()
        {
            _authorityCheck = true;
            
            if (_instance != null)
            {
                Debug.LogError($"There is more than one instance of {this}");
                isBeingDestroyed = true;
                Destroy(this);
                return;
            }
            
            _instance = GetComponent<UserInstanceManager>();
            isBeingDestroyed = false;
            
            if(_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            isBeingDestroyed = true;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer && !IsHost) return;

            base.OnNetworkSpawn();
            NetworkManager.Singleton.OnClientConnectedCallback += CreateUserInstance;
            NetworkManager.Singleton.OnClientDisconnectCallback += DestroyUserInstance;
        }
        
        public override void OnNetworkDespawn()
        {
            if (!IsServer && !IsHost) return;
            
            base.OnNetworkDespawn();
            NetworkManager.Singleton.OnClientConnectedCallback -= CreateUserInstance;
            NetworkManager.Singleton.OnClientDisconnectCallback -= DestroyUserInstance;
        }

        
        private void CreateUserInstance(ulong clientId)
        {
            if (_userInstances.ContainsKey(clientId))
            {
                Debug.LogError($"The client {clientId} has already a userInstance registered");
                return;
            }

            // Spawn UserInstance 
            UserInstance userInstance = Instantiate(_userInstancePrefab); 
            userInstance.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
                
            _userInstances.Add(clientId, userInstance);
        }

        
        
        private void DestroyUserInstance(ulong clientId)
        {
            if (_userInstances.ContainsKey(clientId) == false)
            {
                Debug.LogError($"The client {clientId} has no userInstance registered");
                return;
            }
        
            UserInstance userInstance = _userInstances[clientId];
            userInstance.GetComponent<NetworkObject>().Despawn(true);
            
            _userInstances.Remove(clientId);
        }
        
        public UserInstance GetUserInstance(ulong clientId)
        {
            if (_userInstances.TryGetValue(clientId, out UserInstance userInstance))
            {
                return userInstance;
            }

            Debug.LogError($"The client {clientId} has no userInstance registered");
            return null;
        }
        
        public UserInstance GetUserInstance(string clientName)
        {
            foreach (UserInstance userInstance in _userInstances.Values)
            {
                if (userInstance.name == clientName) return userInstance;
            }

            Debug.LogError($"The client {clientName} has no userInstance registered");
            return null;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticVariables()
        {
            _instance = null;
        }
    }
}
