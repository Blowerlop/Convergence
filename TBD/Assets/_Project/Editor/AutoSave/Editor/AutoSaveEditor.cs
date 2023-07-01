using System;
using System.Collections;
using System.Threading.Tasks;
using ParrelSync;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project
{
    
    public static class AutoSaveEditor
    {
        [InitializeOnLoadMethod]
        private static void Subscribe()
        {
            EditorApplication.playModeStateChanged += OnEnteredEditMode;
            EditorApplication.quitting += OnEditorQuitting;
        }

        private static void OnEnteredEditMode(PlayModeStateChange state)
        {
            if (ClonesManager.IsClone()) return;
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();
                Debug.Log($"Auto-saved on EnteredEditMode at {DateTime.Now:h:mm:ss tt}");
            }
        }

        private static void OnEditorQuitting()
        {
            EditorApplication.playModeStateChanged -= OnEnteredEditMode;
            EditorApplication.quitting -= OnEditorQuitting;
        }
    }
}
