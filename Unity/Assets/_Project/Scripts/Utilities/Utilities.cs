using System;
using System.Collections;
using System.Linq;
using GRPCClient;
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
            if (Physics.Raycast(ray, out RaycastHit hitInfo,100, layerMask))
            {
                position = hitInfo.point;
                return true;
            }
            
            position = Vector3.zero;
            return false;
        }

        public static bool GetFirstHitFromMouse(Camera camera, LayerMask layerMask, out RaycastHit hit)
        {
            Vector3 mousePosition = Mouse.current.position.value;
            mousePosition.z = camera.nearClipPlane;

            Ray ray = camera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo,100, layerMask))
            {
                hit = hitInfo;
                return true;
            }
            
            hit = default;
            return false;
        }
        
        public static Vector3 GrpcToUnityVector3(GRPC_Vector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// /!\ Editor only
        /// </summary>
        /// <param name="type"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] FindAssetsByType<T>(Type type)  
        {
            string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");
            
            return guids.Select(AssetDatabase.GUIDToAssetPath)
                        .Select(path => AssetDatabase.LoadAssetAtPath(path, type))
                        .Cast<T>()
                        .ToArray();
        }
        
        /// <summary>
        /// /!\ Editor only
        /// </summary>
        /// <param name="type"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] FindAssetsByType<T>() where T : UnityEngine.Object {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
            
            return guids.Select(AssetDatabase.GUIDToAssetPath)
                        .Select(AssetDatabase.LoadAssetAtPath<T>)
                        .ToArray();
        }
        #endif
    }
}
