using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Localization
{
    public class LocalizationManager : MonoSingleton<LocalizationManager>
    {
        private string _actualLanguageKey;
        private LanguageSo _actualLanguageBank;
        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();
        }

        public string GetTranslation(string key)
        {
            if (_actualLanguageBank == null || _actualLanguageBank.languageKey != _actualLanguageKey)
            {
                _actualLanguageBank = (LanguageSo)Resources.Load("" + _actualLanguageKey, typeof(LanguageSo));
            }

            if (_actualLanguageBank.LanguageDict.TryGetValue(key, out var value))
            {
                return value;
            }

            Debug.LogWarning($"Translation: the key {key} does not exist");
            return null;
        }
    }
}
