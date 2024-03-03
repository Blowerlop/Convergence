using System.Linq;
using Project.Extensions;
using TMPro;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Project.Localization
{
    public class LButton : Button
    {
        [HideInInspector] public TMPro.TMP_Text buttonText;
        [HideInInspector] public string translationKey;

        protected override void OnValidate()
        {
            base.OnValidate();
            LocalizationManager.GetTranslation(translationKey);
        }
    }
    [CustomEditor(typeof(LButton))]
    public class MenuButtonEditor : ButtonEditor
    {
        private bool _showLoca;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            LButton targetMenuButton = (LButton)target;

            _showLoca = EditorGUILayout.BeginFoldoutHeaderGroup(_showLoca,
                "Translation : " + targetMenuButton.translationKey);
            if (_showLoca)
            {
                targetMenuButton.buttonText =
                    (TMP_Text)EditorGUILayout.ObjectField("ButtonText", targetMenuButton.buttonText, typeof(TMP_Text),
                        true);

                var keys = LocalizationManager.GetKeys();
                if (keys != null && keys.Contains(targetMenuButton.translationKey))
                    targetMenuButton.translationKey =
                        EditorGUILayout.Popup("Translation Key",
                            keys.FindIndex(x => x == targetMenuButton.translationKey),
                            LocalizationManager.GetKeys()).ToString();

                var languages = LocalizationManager.GetLanguagesStrings();
                LocalizationManager.ActualLanguageKey =
                    languages.GetValue(EditorGUILayout.Popup("Language Key",
                        languages.FindIndex(x =>
                            (LocalizationManager.ActualLanguageKey == null) ||
                            (x == LocalizationManager.ActualLanguageKey)),
                        languages)).ToString();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
