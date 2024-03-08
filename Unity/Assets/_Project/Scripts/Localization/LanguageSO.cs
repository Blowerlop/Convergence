using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project.Localization
{
    [CreateAssetMenu(fileName = "new bank", menuName = "Localization/LanguageBank")]
    public class LanguageSo : ScriptableObject
    {
        public string languageKey;
        public Dictionary<string, string> LanguageDict = new();

        public string actualKey;

        public int GetIndex(string searchedKey)
        {
            int index = 0;
            foreach (var element in LanguageDict)
            {
                if (element.Key == searchedKey)
                {
                    return index;
                }

                index++;
            }

            return 0;
        }
        public string GetKey(int searchedElement)
        {
            int index = 0;
            foreach (var element in LanguageDict)
            {
                if (index == searchedElement)
                {
                    return element.Key;
                }

                index++;
            }

            return "";
        }
    }

    [CustomEditor(typeof(LanguageSo))]
    public class LanguageSoEditor : Editor
    {
        private bool _showLoca;

        public override void OnInspectorGUI()
        {
            LanguageSo targetSo = (LanguageSo)target;
            if (targetSo.LanguageDict.Count == 0)
            {
                LocalizationManager.LoadJson();
            }
            targetSo.languageKey = EditorGUILayout.TextField("Language Key", targetSo.languageKey);
            targetSo.actualKey = EditorGUILayout.TextField("Key", targetSo.actualKey);
            if (GUILayout.Button("Add Key"))
            {
                LocalizationManager.AddKey(targetSo.actualKey);
            }
            if (GUILayout.Button("Save"))
            {
               LocalizationManager.SaveJson();
            }
            serializedObject.ApplyModifiedProperties();
            
        }
    }
}
