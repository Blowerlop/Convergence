using System;
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
            EditorApplication.playModeStateChanged += OnExitingEditMode;
            EditorApplication.quitting += OnEditorQuitting;
        }

        private static void OnExitingEditMode(PlayModeStateChange state)
        {
            if (ClonesManager.IsClone()) return;
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();
                Debug.Log($"Auto-saved on EnteredEditMode at {DateTime.Now:h:mm:ss tt}");
            }
        }

        private static void OnEditorQuitting()
        {
            EditorApplication.playModeStateChanged -= OnExitingEditMode;
            EditorApplication.quitting -= OnEditorQuitting;
        }
    }
}
