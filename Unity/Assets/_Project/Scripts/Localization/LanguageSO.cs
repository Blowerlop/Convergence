using System.Collections.Generic;
using UnityEngine;

namespace Project.Localization
{
    [CreateAssetMenu(fileName = "new bank", menuName = "Localization/LanguageBank")]
    public class LanguageSo : ScriptableObject
    {
        public string languageKey;
        public Dictionary<string, string> LanguageDict = new();
    }
}
