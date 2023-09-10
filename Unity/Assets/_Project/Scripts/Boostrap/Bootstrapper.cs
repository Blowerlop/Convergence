using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project
{
    public class Bootstrapper : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ExecuteBeforeSceneLoad()
        {
            TryLoadBootstrapPrefab();
        }

        [RuntimeInitializeOnLoadMethod]
        public static void ExecuteAfterSceneLoad()
        {
            // TryLoadBootstrapScene();
        }

        private static void TryLoadBootstrapPrefab()
        {
            try
            {
                DontDestroyOnLoad(Instantiate(Resources.Load("Systems")));
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

            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(sceneBuildIndex)) return;
            
            
            SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Additive);
        }
    }
}
