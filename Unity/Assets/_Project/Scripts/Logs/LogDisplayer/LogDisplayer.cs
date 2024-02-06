using System.Collections.Generic;
using DG.Tweening;
using Project.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    public class LogDisplayer : MonoSingleton<LogDisplayer>
    {
        [SerializeField] private bool _defaultEnableState = false;
        [SerializeField] private Transform parent;
        [SerializeField] private TMP_Text logTemplate;
        [SerializeField] private float _timeBeforeDisappearing = 5.0f;
        [SerializeField] private float _fadeDuration = 2.0f;
        private List<Tween> _tweens = new List<Tween>();
        
        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();
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
            _tweens.ForEach(sequence => sequence?.Kill());
            _tweens.Clear();
            instance.parent.DestroyChildren();
        }

        private void ComputeHideLog(Graphic logInstance)
        {
            Sequence sequence = DOTween.Sequence();
            _tweens.Add(sequence);
            
            sequence.AppendInterval(_timeBeforeDisappearing)
                    .Append(logInstance.DOFade(0.0f, _fadeDuration))
                    .OnComplete(() =>
                    {
                        _tweens.Remove(sequence);
                        Destroy(logInstance.gameObject); 
                    });
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

            TMP_Text logInstance = Instantiate(logTemplate, parent);
            logInstance.alpha = 1.0f;
            logInstance.text = $"<color=#{ColorUtility.ToHtmlStringRGB(logColor)}>{condition}</color>\n";
            ComputeHideLog(logInstance);
        }

        [ConsoleCommand("logDisplay", "Display all new logs on an UI that is directly in the game view.")]
        private static void EnableLogDisplay(bool state)
        {
            if (instance.parent.gameObject.activeInHierarchy == state) return;
            
            instance.DestroyLogs();
            instance.parent.gameObject.SetActive(state);            
        }
    }
}
