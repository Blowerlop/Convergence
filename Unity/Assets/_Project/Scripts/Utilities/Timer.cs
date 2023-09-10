using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    public enum TimeType
    {
        Scaled,
        Unscaled
    }
    public class Timer
    {
        [ShowInInspector] public float timer { get; private set; }
        public bool isTimerRunning => _timerCoroutine != null;
        private Coroutine _timerCoroutine;
        private MonoBehaviour _monoBehaviour;

        public void StartSimpleTimerScaled(MonoBehaviour monoBehaviour, float timeInSeconds, TimeType timerType = TimeType.Scaled, bool forceStart = false) =>
            StartTimerWithCallback(monoBehaviour, timeInSeconds, null, timerType, forceStart);
        
        public void StartTimerWithCallback(MonoBehaviour monoBehaviour, float timeInSeconds, Action callback, TimeType timerType = TimeType.Scaled, bool forceStart = false)
        {
            if (isTimerRunning)
            {
                if (forceStart)
                {
                    StopTimer();
                    return;
                }
                
                Debug.Log("A timer is already in progress");
                return;
            }

            _monoBehaviour = monoBehaviour;
            _timerCoroutine = monoBehaviour.StartCoroutine(TimerCoroutine(timeInSeconds, timerType, callback));
        }
        
        private IEnumerator TimerCoroutine(float timeInSeconds, TimeType timerType, Action callback)
        {
            timer = timeInSeconds;

            if (timerType == TimeType.Scaled)
            {
                while (timer > 0.0f)
                {
                    timer -= Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                while (timer > 0.0f)
                {
                    timer -= Time.unscaledDeltaTime;
                    yield return null;
                }
            }
            
            callback?.Invoke();
            _timerCoroutine = null;
            _monoBehaviour = null;
        }

        private void StopTimer()
        {
            if (_monoBehaviour != null && _timerCoroutine != null)
            {
                _monoBehaviour.StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
                _monoBehaviour = null;
            }
        }

        public float GetTimeRemaining() => timer;


        
        // -------------------------------------------------------------------------------------------------------------
        // Static Section

        public static Coroutine StartTimerWithCallbackScaled(MonoBehaviour monoBehaviour, float timeInSeconds, Action callback)
        {
            return monoBehaviour.StartCoroutine(TimerWithCallbackScaledCoroutine(timeInSeconds, callback));
        }
        
        public static Coroutine StartTimerWithCallbackUnscaled(MonoBehaviour monoBehaviour, float timeInSeconds, Action callback)
        {
            return monoBehaviour.StartCoroutine(TimerWithCallbackUnscaledCoroutine(timeInSeconds, callback));
        }

        private static IEnumerator TimerWithCallbackScaledCoroutine(float timeInSeconds, Action callback)
        {
            yield return new WaitForSeconds(timeInSeconds);
            callback?.Invoke();
        }
        
        private static IEnumerator TimerWithCallbackUnscaledCoroutine(float timeInSeconds, Action callback)
        {
            yield return new WaitForSecondsRealtime(timeInSeconds);
            callback?.Invoke();
        }
    }
}
