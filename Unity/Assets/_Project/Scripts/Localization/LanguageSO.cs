using System.Collections.Generic;
using UnityEngine;

namespace Project.Localization
{
    public class LanguageSo : ScriptableObject
    {
        public string languageKey;
        public Dictionary<string, string> LanguageDict;
    }
}
