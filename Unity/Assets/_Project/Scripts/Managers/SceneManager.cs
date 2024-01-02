using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project
{
    public class SceneManager : NetworkBehaviour
    {
        [ClearOnReload] private static LoadingScreenParameters _currentLoadingScreenParameters;
        
        public override void OnNetworkSpawn()
        {
            NetworkManager.Singleton.SceneManager.OnLoad += OnLoad;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        }
        
        public override void OnNetworkDespawn()
        {
            NetworkManager.Singleton.SceneManager.OnLoad -= OnLoad;
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        }

        
        private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            LoadingScreenManager.Hide();  
        }
        
        private void OnLoad(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
        {
            LoadingScreenManager.Show(_currentLoadingScreenParameters/*, asyncOperation*/);
        }

        public static void Network_LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode, LoadingScreenParameters loadingScreenParameters)
        {
            _currentLoadingScreenParameters = loadingScreenParameters;
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, loadSceneMode);
        }
        
        public static void LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode)
        {
            AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            LoadingScreenManager.Show(new LoadingScreenParameters(), asyncOperation);
        }
    }
}