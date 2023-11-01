using System;
using UnityEngine;

namespace Project
{
    using UnityEngine;

    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static bool dontDestroyOnLoad = true;
        public static bool isBeingDestroyed { get; private set; }
        
        private static T _instance = null;
        public static T instance {
            get 
            { 
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

        protected virtual void OnDestroy()
        {
            isBeingDestroyed = true;
        }

        public static bool IsInstanceAlive() => _instance != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            _instance = null;
        }
    }
}
