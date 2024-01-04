using Unity.Netcode;

namespace Project
{
    using UnityEngine;

    [DefaultExecutionOrder(-1)]
    public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        protected static bool authorityCheck = false;
        
        protected static bool dontDestroyOnLoad = true;
        public static bool isBeingDestroyed { get; private set; }
        
        private static T _instance = null;
        public static T instance {
            get
            {
                if (authorityCheck && CanClientRead() == false)
                {
                    Debug.LogError("Trying to access a NetworkSingleton instance with Authority Checked as non Server Client");
                    return null;
                }
                
                if(_instance == null){
                    _instance = FindObjectOfType<T>();
                    if(_instance == null){
                        GameObject singletonObj = new GameObject();
                        singletonObj.name = typeof(T).ToString();
                        _instance = singletonObj.AddComponent<T>();
                    } 
                }
                
                return _instance;
            }
        }
        
        protected virtual void Awake(){
            if (_instance != null)
            {
                Debug.LogError($"There is more than one instance of {this}");
                isBeingDestroyed = true;
                Destroy(this);
                return;
            }
            
            _instance = GetComponent<T>();
            isBeingDestroyed = false;
            
            if(dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            isBeingDestroyed = true;
        }

        public static bool IsInstanceAlive() => _instance != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticVariables()
        {
            _instance = null;
        }

        private static bool CanClientRead()
        {
            return NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId;
        }
    }
}
