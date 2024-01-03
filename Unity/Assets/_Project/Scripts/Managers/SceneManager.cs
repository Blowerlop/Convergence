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

        
        private void OnLoad(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
        {
            if (IsClient)
            {
                LoadingScreenManager.Show(_currentLoadingScreenParameters, asyncOperation);
            }
        }
        
        private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (IsClient)
            {
                UpdateUserInstanceSceneServerRpc(clientId, sceneName);
                LoadingScreenManager.Hide();  
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void UpdateUserInstanceSceneServerRpc(ulong clientId, string sceneName)
        {
            if (UserInstanceManager.instance.TryGetUserInstance((int)clientId, out UserInstance userInstance))
            {
                userInstance.SetScene(sceneName);
            }
            else
            {
                Debug.LogError("[SceneManager/OnLoadComplete] UserInstance is null");
            }
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