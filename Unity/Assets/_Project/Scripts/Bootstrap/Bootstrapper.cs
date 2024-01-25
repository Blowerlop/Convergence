using UnityEngine;

namespace Project
{
    public static class Bootstrapper
    {
        private static SOBootstrap _bootstrap;
        
        
        public static SOBootstrap GetScriptableObject()
        {
            if (_bootstrap == null) _bootstrap = Resources.Load<SOBootstrap>("Bootstrap");
                
            return _bootstrap;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void SubsystemRegistration()
        {
            Execute(GetScriptableObject().subsystemRegistration);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void AfterAssembliesLoaded()
        {
            Execute(GetScriptableObject().afterAssembliesLoaded);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void BeforeSplashScreen()
        {
            Execute(GetScriptableObject().beforeSplashScreen);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void BeforeSceneLoad()
        {
            Execute(GetScriptableObject().beforeSceneLoad);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void AfterSceneLoad()
        {
            Execute(GetScriptableObject().afterSceneLoad);
        }

        private static void Execute(Object[] objects)
        {
            foreach (Object obj in objects)
            {
                Object.DontDestroyOnLoad(Object.Instantiate(obj));
            }
        }
    }
}
