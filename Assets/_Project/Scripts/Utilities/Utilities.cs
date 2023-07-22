using System;
using System.Collections;
using UnityEngine;

namespace Project
{
    public class Utilities : MonoBehaviour
    {
        public static IEnumerator LerpInTimeCoroutine(float timeInSeconds, float from, float to, Action<float> callback, Action onFinishCallback = null)
        {
            float timer = 0.0f;
            while (timer < timeInSeconds)
            {
                timer += Time.deltaTime;
                callback.Invoke(Mathf.Lerp(from, to, timer / timeInSeconds));
                yield return null;
            }
            
            onFinishCallback?.Invoke();
        }
        
        public static IEnumerator WaitForSecondsAndDoActionCoroutine(float timeInSeconds, Action action)
        {
            yield return new WaitForSeconds(timeInSeconds);
            action.Invoke();
        }
        
        public static IEnumerator WaitForFramesAndDoActionCoroutine(int frames, Action action)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }
            
            action.Invoke();
        }
        
        public static IEnumerator WaitForEndOfFrameAndDoActionCoroutine(Action action)
        {
            yield return new WaitForEndOfFrame();            
            action.Invoke();
        }

        /// <summary>
        /// This script only works if there is only one layer selected
        /// </summary>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static int LayerToInt(LayerMask layerMask)
        {
            return Mathf.RoundToInt(Mathf.Log(layerMask.value, 2));
        }
        
            
        public static void SwitchLayerInChildren(GameObject gameObject, LayerMask layerMask)
        {
            Transform[] children = gameObject.GetComponentsInChildren<Transform>();
            for (int i = 0; i < children.Length; i++)
            {
                children[i].gameObject.layer = LayerToInt(layerMask);
            }
        }
    }
}
