using System.Collections;
using Project.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    public struct LoadingScreenParameters
    {
        public readonly Sprite backgroundSprite;
        public readonly Color? backgroundColor;
    }
    
    public class LoadingScreenManager : MonoBehaviour
    {
        [SerializeField, RequiredIn(PrefabKind.PrefabAsset), AssetsOnly] private LoadingScreen _loadingScreenPrefab;
        private static LoadingScreenManager _instance;
        private LoadingScreen _loadingScreenInstance;
        private Coroutine _showCoroutine;


        private void Awake()
        {
            _instance = this;
        }


        public static void Show(LoadingScreenParameters loadingScreenParameters, AsyncOperation asyncOperation = null)
        {
            _instance.ShowBehaviour(loadingScreenParameters, asyncOperation);
        }
        
        [Button]
        public void ShowBehaviour(LoadingScreenParameters loadingScreenParameters, AsyncOperation asyncOperation = null)
        {
            if (_loadingScreenInstance != null) return;

            _showCoroutine = StartCoroutine(ShowBehaviourCoroutine(loadingScreenParameters, asyncOperation));
        }

        private IEnumerator ShowBehaviourCoroutine(LoadingScreenParameters loadingScreenParameters, AsyncOperation asyncOperation = null)
        {
            _loadingScreenInstance = Instantiate(_loadingScreenPrefab);
            _loadingScreenInstance.UpdateLoadingScreen(loadingScreenParameters);

            if (asyncOperation != null && _loadingScreenInstance.transform.TryGetComponentInChildren(out LoadingBar loadingBar))
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
