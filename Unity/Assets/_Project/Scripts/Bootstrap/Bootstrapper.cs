using System.Linq;
using UnityEngine;

namespace Project
{
    public static class Bootstrapper
    {
        private static SOBootstrap _instance;

        public static SOBootstrap instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.FindObjectsOfTypeAll<SOBootstrap>().FirstOrDefault();
                }

                return _instance;
            }
        }
        
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void SubsystemRegistration()
        {
            Execute(instance.subsystemRegistration);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void AfterAssembliesLoaded()
        {
            Execute(instance.afterAssembliesLoaded);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void BeforeSplashScreen()
        {
            Execute(instance.beforeSplashScreen);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void BeforeSceneLoad()
        {
            Execute(instance.beforeSceneLoad);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void AfterSceneLoad()
        {
            Execute(instance.afterSceneLoad);
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
