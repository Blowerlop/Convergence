using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace Project
{
    public class LogDisplayer : MonoSingleton<LogDisplayer>
    {
        [SerializeField] private bool _defaultEnableState = true;
        [SerializeField] private Transform parent;
        [SerializeField] private TMP_Text logTemplate;
        [SerializeField] private float _timeBeforeDisappearing = 5.0f;
        private WaitForSeconds _waitForSeconds;
        [SerializeField] private float _fadeDuration = 2.0f;
        private ObjectPool<TMP_Text> _pool;

        
        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();
            
            _waitForSeconds = new WaitForSeconds(_timeBeforeDisappearing);
            _pool = new ObjectPool<TMP_Text>(
                LogInstancer,
                actionOnRelease: text => text.gameObject.SetActive(false),
                actionOnGet: text => text.gameObject.SetActive(true),
                maxSize: 20
            );
        }
        
        private void Start()
        {
            EnableLogDisplay(_defaultEnableState);
        }

        private void OnEnable()
        {
            Application.logMessageReceived += DisplayLog;
        }
        
        private void OnDisable()
        {
            Application.logMessageReceived -= DisplayLog;
            DestroyLogs();
        }

        
        private void DestroyLogs()
        {
            _pool.Dispose();
        }

        private IEnumerator HideLogCoroutine(TMP_Text logInstance)
        {
            yield return _waitForSeconds;
            logInstance.DOFade(0.0f, _fadeDuration).OnComplete(() => _pool.Release(logInstance));
        }
        
        private void DisplayLog(string condition, string trace, LogType logType)
        {
            Color logColor;
            switch (logType)
            {
                case LogType.Log:
                    logColor = CustomLogger.logColor;
                    break;
                
                case LogType.Warning:
                    logColor = CustomLogger.logWarningColor;
                    break;
                
                // All the other LogType
                default:
                    logColor = CustomLogger.logErrorColor;
                    break;
            }

            TMP_Text logInstance = _pool.Get();
            logInstance.alpha = 1.0f;
            logInstance.text = $"<color=#{ColorUtility.ToHtmlStringRGB(logColor)}>{condition}</color>\n";
            StartCoroutine(HideLogCoroutine(logInstance));
        }

        [ConsoleCommand("logDisplay", "Display all new logs on an UI that is directly in the game view.")]
        private static void EnableLogDisplay(bool state)
        {
            instance.gameObject.SetActive(state);
        }

        private TMP_Text LogInstancer()
        {
            return Instantiate(logTemplate, parent);
        }
    }
}
