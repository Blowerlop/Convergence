using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

//
// Inspired of https://gist.github.com/Lazersquid/4f04327da0741f2e1dea5038026701f2
//
namespace Project
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Reference Cache", fileName = "SO References Cache")]
    public class SOScriptableObjectReferencesCache : ScriptableObject
    {
#if UNITY_EDITOR
        [field: Title("Configuration")]
        [field: SerializeField] public bool autoFetchInPlaymode { get; private set; } = true;
#endif
        
        [Title("Data")]
        // I can't make it dictionary because Unity can't serialize them 
        [field: SerializeField, ReadOnly] private List<SOCacheEntry> _scriptableObjectsCache;

        
        public T[] GetScriptableObjects<T>()
        {
            return _scriptableObjectsCache.Find(soCacheEntry => soCacheEntry.typeName == typeof(T).Name).scriptableObjects.Cast<T>().ToArray();
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// /!\ Never use this to get the asset instance other than in editor.
        /// If you want to get the asset instance other than in editor, you need to get the direct reference of it.
        /// </summary>
        /// <returns></returns>
        public static SOScriptableObjectReferencesCache GetAssetInstance() 
        {
            // Should exist only one asset of this
            SOScriptableObjectReferencesCache[] allAssetInstances = Utilities.FindAssetsByType<SOScriptableObjectReferencesCache>();
            if (allAssetInstances == null)
            {
                Debug.LogError("Cannot find ScriptableObjectReferencesCache instance");
                return null;
            }

            if (allAssetInstances.Length > 1)
            {
                Debug.LogWarning("More than one asset of ScriptableObjectReferencesCache, destroying them...");
                
                string[] assetsPath = AssetDatabase.FindAssets($"t:{typeof(SOScriptableObjectReferencesCache)}").Skip(1).Select(AssetDatabase.GUIDToAssetPath).ToArray();
                
                AssetDatabase.DeleteAssets(assetsPath, new List<string>());
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.LogWarning("Done !");
            }
            
            return allAssetInstances.First();
        }
        
        [ButtonGroup]
        public static void FetchReferences()
        {
            ClearReferences();

            SOScriptableObjectReferencesCache assetInstance = GetAssetInstance();
            assetInstance._scriptableObjectsCache = new List<SOCacheEntry>();
            
            var types = GetSoTypesWithInterface<IScriptableObjectSerializeReference>();
            foreach (Type type in types)
            {
                var scriptableObjects = Utilities.FindAssetsByType<ScriptableObject>(type);
                assetInstance._scriptableObjectsCache.Add(new SOCacheEntry(type.Name, scriptableObjects));
            }
            
            assetInstance.ForceSaveOnDisk();
        }

        [ButtonGroup]
        private static void ClearReferences()
        {
            SOScriptableObjectReferencesCache assetInstance = GetAssetInstance();
            assetInstance._scriptableObjectsCache = null;
        }
        
        private static Type[] GetSoTypesWithInterface<T>()
        {
            return AssemblyUtilities.GetTypes(AssemblyTypeFlags.All)
                .Where(t =>
                    !t.IsAbstract &&
                    !t.IsGenericType &&
                    typeof(T).IsAssignableFrom(t) &&
                    t.IsSubclassOf(typeof(ScriptableObject)))
                .ToArray();
        }
        
        [Button]
        [ParrelSyncIgnore]
        private void ForceSaveOnDisk()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        #endif 
    }
    
    
    
    #if UNITY_EDITOR
    public class ScriptableObjectReferencesCacheEditor : IPreprocessBuildWithReport
    {
        [InitializeOnLoadMethod]
        private static void Subscribe()
        {
            EditorApplication.playModeStateChanged += OnExitingEditMode;
        }

        private static void OnExitingEditMode(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode) return;
            
            if (SOScriptableObjectReferencesCache.GetAssetInstance().autoFetchInPlaymode)
            {
                SOScriptableObjectReferencesCache.FetchReferences();
            }
        }
        
        public int callbackOrder { get; }
        public void OnPreprocessBuild(BuildReport report)
        {
            SOScriptableObjectReferencesCache.FetchReferences();
        }
    }
#endif
    
    [Serializable]
    public class SOCacheEntry
    {
        [field: SerializeField] public string typeName { get; private set; }
        [field: SerializeField] public ScriptableObject[] scriptableObjects { get; private set; }

        
        public SOCacheEntry(string typeName, ScriptableObject[] scriptableObjects)
        {
            this.typeName = typeName;
            this.scriptableObjects = scriptableObjects;
        }
    }
    
    public interface IScriptableObjectSerializeReference
    {
        
    }
}