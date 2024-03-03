using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Project.Localization
{
    public class LButton : Button
    {
        public TMPro.TMP_Text buttonText;
        public string translationKey;

        protected override void OnValidate()
        {
            base.OnValidate();
            LocalizationManager.instance.GetTranslation(translationKey);
        }
    }
    [CustomEditor(typeof(LButton))]
    public class MenuButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LButton targetMenuButton = (LButton)target;

            targetMenuButton.buttonText =
                (TMP_Text)EditorGUILayout.ObjectField(targetMenuButton.buttonText, typeof(TMP_Text), true);
            DrawDefaultInspector();
        }
    }
}
