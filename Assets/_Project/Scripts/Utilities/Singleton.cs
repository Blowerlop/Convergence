using System;

namespace Project
{
    using UnityEngine;

    public class Singleton<T> : MonoBehaviour where T: MonoBehaviour
    {
        protected static bool keepAlive = true;
        protected static bool isBeingDestroyed = false;
        
        private static T _instance = null;
        public static T instance {
            get { 
                if(_instance == null){
                    _instance = FindObjectOfType<T>();
                    if(_instance == null){
                        var singletonObj = new GameObject();
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
        
            if(keepAlive)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            isBeingDestroyed = false;
        }

        public static bool IsInstanceAlive{
            get { return _instance != null; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            _instance = null;
        }
    }
}
