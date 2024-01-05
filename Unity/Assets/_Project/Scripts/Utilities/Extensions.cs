using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public enum EAxis
{
    X,
    Y,
    Z
}

namespace Project.Extensions
{
    public static class TransformExtensions
    {
        public static List<Transform> GetChildrenFirstDepth(this Transform transform)
        {
            return GetComponentsInChildrenFirstDepthWithoutTheParent<Transform>(transform);
        }

        public static List<Transform> GetChildrenRecursively(this Transform transform, List<Transform> children = null)
        {
            return GetComponentsInChildrenRecursivelyWithoutTheParent<Transform>(transform);
        }

        public static List<T> GetComponentsInChildrenFirstDepthWithoutTheParent<T>(this Transform transform)
            where T : Object
        {
            List<T> children = new List<T>();

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out T component))
                {
                    children.Add(component);
                }
            }

            return children;
        }

        public static List<T> GetComponentsInChildrenRecursivelyWithoutTheParent<T>(this Transform transform) where T : Object
        {
            return GetComponentsInChildrenRecursivelyWithoutTheParent(transform, new List<T>());
        }
        
        private static List<T> GetComponentsInChildrenRecursivelyWithoutTheParent<T>(this Transform transform,
            List<T> children) where T : Object
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                child.GetComponentsInChildrenRecursivelyWithoutTheParent(children);
                if (child.TryGetComponent(out T tChild))
                {
                    children.Add(tChild);
                }
            }

            return children;
        }

        public static bool TryGetComponentInChildren<T>(this Transform transform, out T component)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(transform);
            while (queue.Count != 0)
            {
                Transform tempNode = queue.Dequeue();
                if (tempNode.TryGetComponent(out T tChild))
                {
                    component = tChild;
                    return true;
                }

                for (int i = 0; i < tempNode.childCount; i++)
                {
                    queue.Enqueue(tempNode.GetChild(i));
                }
            }

            component = default;
            return false;
        }
        
        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isEditor)
                {
                    Object.DestroyImmediate(transform.GetChild(i).gameObject);
                }
                else
                {
                    Object.Destroy(transform.GetChild(i).gameObject);
                }
            }
        }
    }

    public static class RigidbodyExtensions
    {
        public static void ResetVelocities(this Rigidbody rigidbody)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }

    public static class CollectionsExtensions
    {
        public static void ForEach<T>(this IList<T> target, Action<T> action)
        {
            for (int i = 0; i < target.Count; i++)
            {
                action.Invoke(target[i]);
            }
        }

        public static void ForEach<T1, T2>(this Dictionary<T1, T2> target, Action<T1, T2> action)
        {
            foreach (KeyValuePair<T1, T2> kvp in target)
            {
                action.Invoke(kvp.Key, kvp.Value);
            }
        }
    }

    public static class StringExtensions
    {
        public static string SeparateContent(this string text)
        {
            return string.Concat(text.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        }
        
        public static string ExtractNumber(this string text)
        {
            var match = Regex.Match(text, @"([-+]?[0-9]*\.?[0-9]+)");
            if (match.Success)
                return (match.Groups[1].Value);

            return string.Empty;
        }
        
        public static string RemoveFirstLine(this string text)
        {
            return text.Substring(text.IndexOf("\n", StringComparison.Ordinal)+1);
        }
        
        public static string FollowCasePattern(this string text, string target)
        {
            int textLength = text.Length;
            if (textLength < target.Length) throw new ArgumentOutOfRangeException();
            
            StringBuilder stringBuilder = new StringBuilder();
            
            for (int i = 0; i < textLength; i++)
            {
                if (char.IsUpper(target[i]))
                {
                    stringBuilder.Append(char.ToUpper(text[i]));
                }
                else
                {
                    stringBuilder.Append(char.ToLower(text[i]));
                }
            }

            return stringBuilder.ToString();
        }
        
        public static int ToHashIsSameAlgoOnUnreal(this string stringToHash)
        {
            const int p = 31;
            const long m = (long)1e9 + 9;
            long hashValue = 0;
            long pPow = 1;
            foreach (char c in stringToHash) {
                hashValue = (hashValue + (c - 'a' + 1) * pPow) % m;
                pPow = (pPow * p) % m;
            }
            return (int)hashValue; 
        }
    }

    public static class UnityEventExtensions
    {
        /// <summary>
        /// This extension will permit to add automatically a persistant listener when in editor and add a non-persistant listener when non in editor
        /// </summary>
        public static void AddListenerExtended(this UnityEvent unityEvent, UnityAction call)
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.AddPersistentListener(unityEvent, call);
#else
                    unityEvent.AddListener(call);
#endif
        }

        /// <summary>
        /// This extension will permit to remove automatically a persistant listener when in editor and add a non-persistant listener when non in editor
        /// </summary>
        public static void RemoveListenerExtended(this UnityEvent unityEvent, UnityAction call)
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(unityEvent, call);
#else
                    unityEvent.RemoveListener(call);
#endif
        }

        /// <summary>
        /// This extension will permit to add automatically a persistant listener when in editor and add a non-persistant listener when non in editor
        /// </summary>
        public static void AddListenerExtended<T>(this UnityEvent<T> unityEvent, UnityAction<T> call)
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.AddPersistentListener<T>(unityEvent, call);
#else
                    unityEvent.AddListener(call);
#endif
        }

        /// <summary>
        /// This extension will permit to remove automatically a persistant listener when in editor and add a non-persistant listener when non in editor
        /// </summary>
        public static void RemoveListenerExtended<T>(this UnityEvent<T> unityEvent, UnityAction<T> call)
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(unityEvent, call);
#else
                    unityEvent.RemoveListener(call);
#endif
        }
    }

    public static class UnityObjectExtensions
    {
        public static T IsNull<T>(this T @object) where T : UnityEngine.Object
        {
            return @object == null ? null : @object;
        }
    }

    public static class VectorExtensions
    {
        public static Vector3 RemoveAxis(this Vector3 vector3, EAxis axis)
        {
            switch (axis)
            {
                case EAxis.X:
                    vector3.x = 0.0f;
                    break;
                
                case EAxis.Y:
                    vector3.y = 0.0f;
                    break;
                
                case EAxis.Z:
                    vector3.z = 0.0f;
                    break;
            }

            return vector3;
        }
    }
}
