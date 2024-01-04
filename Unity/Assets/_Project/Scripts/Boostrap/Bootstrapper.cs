using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Project
{
    public class Bootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ExecuteBeforeSceneLoad()
        {
            TryLoadBootstrapPrefab();
            TryLoadBootstrapScene();
        }

        private static void TryLoadBootstrapPrefab()
        {
            try
            {
                Object.DontDestroyOnLoad(Object.Instantiate(Resources.Load("Systems")));
            }
            catch
            {
                Debug.LogError("There is no prefab named 'Systems' in resources");
            }
        }
        
        private static void TryLoadBootstrapScene()
        {
            int sceneBuildIndex = SceneUtility.GetBuildIndexByScenePath("Bootstrap");
            
            if (sceneBuildIndex == -1)
            {
                Debug.LogError("There is no scene named 'Bootstrap'");
                return;
            }
            
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene() == UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(sceneBuildIndex)) return;
            
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Additive);
            
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) =>
            {
                OnSceneLoaded_UnloadScene(scene);
            };


            void OnSceneLoaded_UnloadScene(Scene scene)
            {
                if (scene.buildIndex != sceneBuildIndex) return;
                
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
                UnityEngine.SceneManagement.SceneManager.sceneLoaded -= (scene, mode) => OnSceneLoaded_UnloadScene(scene);
            }
        }
    }
}
