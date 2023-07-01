using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Project
{
    public static class Extensions
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

        public static List<T> GetComponentsInChildrenFirstDepthWithoutTheParent<T>(this RectTransform rectTransform)
            where T : Object
        {
            List<T> children = new List<T>();

            for (int i = 0; i < rectTransform.childCount; i++)
            {
                if (rectTransform.GetChild(i).TryGetComponent(out T component))
                {
                    children.Add(component);
                }
            }

            return children;
        }

        public static List<T> GetComponentsInChildrenRecursivelyWithoutTheParent<T>(this Transform transform,
            List<T> children = null) where T : Object
        {
            if (children == null) children = new List<T>();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                child.GetComponentsInChildrenRecursivelyWithoutTheParent(children);
                if (child.TryGetComponent(out T TChild))
                {
                    children.Add(TChild);
                }
            }

            return children;
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

            List<Transform> a = new List<Transform>();
        }

        public static void ResetVelocities(this Rigidbody rigidbody)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

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

        public static string SeparateContent(this string text)
        {
            return string.Concat(text.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        }

        #region UnityEvent

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

        #endregion

        public static string ExtractNumber(this string text)
        {
            var match = Regex.Match(text, @"([-+]?[0-9]*\.?[0-9]+)");
            if (match.Success)
                return (match.Groups[1].Value);

            return "";
        }

        public static bool IsNullOrEmpty(this string @string) => string.IsNullOrEmpty(@string);
    }
}
