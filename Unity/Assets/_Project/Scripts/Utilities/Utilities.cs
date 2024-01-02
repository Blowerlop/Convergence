using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

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
                float linearValue = Mathf.Lerp(from, to, timer / timeInSeconds);
                callback.Invoke(linearValue);
                yield return null;
            }
            
            onFinishCallback?.Invoke();
        }
        
        public static IEnumerator LerpInTimeCoroutine(float timeInSeconds, Vector3 from, Vector3 to, Action<Vector3> callback, Action onFinishCallback = null)
        {
            float timer = 0.0f;
            while (timer < timeInSeconds)
            {
                timer += Time.deltaTime;
                Vector3 linearValue = Vector3.Lerp(from, to, timer / timeInSeconds);
                callback.Invoke(linearValue);
                yield return null;
            }
            
            onFinishCallback?.Invoke();
        }
        
        public static IEnumerator LerpInTimeCoroutine(float timeInSeconds, Quaternion from, Quaternion to, Action<Quaternion> callback, Action onFinishCallback = null)
        {
            float timer = 0.0f;
            while (timer < timeInSeconds)
            {
                timer += Time.deltaTime;
                Quaternion linearValue = Quaternion.Lerp(from, to, timer / timeInSeconds);
                callback.Invoke(linearValue);
                yield return null;
            }
            
            onFinishCallback?.Invoke();
        }

        public static void StartWaitForSecondsAndDoActionCoroutine(MonoBehaviour monoBehaviour, float timeInSeconds, Action action)
        {
            monoBehaviour.StartCoroutine(WaitForSecondsAndDoActionCoroutine(timeInSeconds, action));
        }
        
        public static IEnumerator WaitForSecondsAndDoActionCoroutine(float timeInSeconds, Action action)
        {
            yield return new WaitForSeconds(timeInSeconds);
            action.Invoke();
        }
        
        public static void StartWaitForFramesAndDoActionCoroutine(MonoBehaviour monoBehaviour, int frames, Action action)
        {
            monoBehaviour.StartCoroutine(WaitForFramesAndDoActionCoroutine(frames, action));
        }
        
        public static IEnumerator WaitForFramesAndDoActionCoroutine(int frames, Action action)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }
            
            action.Invoke();
        }

        public static void StartWaitForEndOfFrameAndDoActionCoroutine(MonoBehaviour monoBehaviour, Action action)
        {
            monoBehaviour.StartCoroutine(WaitForEndOfFrameAndDoActionCoroutine(action));
        }
        
        public static IEnumerator WaitForEndOfFrameAndDoActionCoroutine(Action action)
        {
            yield return new WaitForEndOfFrame();            
            action.Invoke();
        }

        public static IEnumerator WaitWhileAndDoAction(Func<bool> predicate, Action action)
        {
            yield return new UnityEngine.WaitWhile(predicate);
            action.Invoke();
        }

        public static void StartWaitUntilAndDoAction(MonoBehaviour monoBehaviour, Func<bool> predicate, Action action)
        {
            monoBehaviour.StartCoroutine(WaitUntilAndDoAction(predicate, action));
        }
        
        public static IEnumerator WaitUntilAndDoAction(Func<bool> predicate, Action action)
        {
            yield return new UnityEngine.WaitUntil(predicate);
            action.Invoke();
        }

        public static bool GetMouseWorldPosition(Camera camera, LayerMask layerMask, out Vector3 position)
        {
            Vector3 mousePosition = Mouse.current.position.value;
            mousePosition.z = camera.nearClipPlane;

            Ray ray = camera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                position = hitInfo.point;
                return true;
            }
            
            position = Vector3.zero;
            return false;
        }
        
        #if UNITY_EDITOR
        public static IEnumerable<T> FindAssetsByType<T>() where T : UnityEngine.Object {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
            foreach (var t in guids) {
                var assetPath = AssetDatabase.GUIDToAssetPath(t);
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null) {
                    yield return asset;
                }
            }
        }
        #endif
    }
}
