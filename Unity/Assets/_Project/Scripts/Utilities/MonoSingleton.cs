namespace Project
{
    using UnityEngine;

    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static bool dontDestroyOnLoad = true;
        
        private static T _instance = null;
        public static T instance {
            get
            {
                if (Application.isPlaying == false) return null;
                
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
                Destroy(this);
                return;
            }
            
            _instance = GetComponent<T>();
            
            if(dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public static bool IsInstanceAlive() => _instance != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticVariables()
        {
            _instance = null;
        }
    }
}
