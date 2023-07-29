using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project
{
    public struct LoadingScreenParameters
    {
        // private readonly 
    }
    
    public class LoadingManager : MonoBehaviour
    {
        [SerializeField, RequiredIn(PrefabKind.PrefabAsset), AssetsOnly] private Transform _loadingScreenPrefab;
        private Transform _loadingScreenInstance;
        private Coroutine _showCoroutine;

        [Button]
        public void Show(LoadingScreenParameters loadingScreenParameters, AsyncOperation asyncOperation = null)
        {
            if (_loadingScreenInstance != null) return;

            _showCoroutine = StartCoroutine(ShowCoroutine(loadingScreenParameters, asyncOperation));
        }

        private IEnumerator ShowCoroutine(LoadingScreenParameters loadingScreenParameters, AsyncOperation asyncOperation = null)
        {
            _loadingScreenInstance = Instantiate(_loadingScreenPrefab);

            if (asyncOperation != null && _loadingScreenInstance.TryGetComponentInChildren(out LoadingBar loadingBar))
            {
                asyncOperation.allowSceneActivation = false;
                
                while (asyncOperation.progress <= 0.9f)
                {
                    loadingBar.UpdateLoadingBar((asyncOperation.progress / 0.9f) * 100);
                    yield return null;
                }

                yield return new WaitForSecondsRealtime(0.1f);
                asyncOperation.allowSceneActivation = true;
            }
        }
        

        [Button]
        public void Hide()
        {
            if (_loadingScreenInstance == null) return;

            Destroy(_loadingScreenInstance.gameObject);
            _loadingScreenInstance = null;

            
            if (_showCoroutine == null) return;
            
            StopCoroutine(_showCoroutine);
            _showCoroutine = null;
        }
    }
}
