using UnityEditor;
using UnityEngine;

namespace Project
{
    public class EnterPlayModeOptions
    {
        [MenuItem("Tools/EnterPlayModeOptions/ToggleEnterPlayModeOptions")]
        public static void ToggleEnterPlayModeOptions()
        {
            EditorSettings.enterPlayModeOptionsEnabled = !EditorSettings.enterPlayModeOptionsEnabled;
            Debug.Log($"EnterPlayModeOptions {EditorSettings.enterPlayModeOptionsEnabled}");
        }
    }
}
