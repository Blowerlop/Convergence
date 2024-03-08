using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Project.Localization
{
    public class LocalizationManager : MonoBehaviour
    {
        public static string ActualLanguageKey;
        private static LanguageSo _actualLanguageBank;
        private static string[] _actualKeys;
        private static LanguageSo[] _languagesSo;

        public static string GetTranslation(string key)
        {
            if(_languagesSo == null)
                RefreshLanguages();
            
            if (_actualLanguageBank == null || _actualLanguageBank.languageKey != ActualLanguageKey)
            {
                _actualLanguageBank = _languagesSo.FirstOrDefault(x=> x.languageKey == ActualLanguageKey);
            }

            if (_actualLanguageBank != null &&_actualLanguageBank.LanguageDict.TryGetValue(key, out var value))
            {
                return value;
            }

            Debug.LogWarning($"Translation: the key {key} does not exist");
            return null;
        }

        public static string[] GetKeys()
        {
            return _actualKeys;
        }

        [MenuItem("Localization/Refresh Banks", priority = 1)]
        public static void RefreshLanguages()
        {
            var langSo = Resources.LoadAll("Localization", typeof(LanguageSo)).Cast<LanguageSo>();
            _languagesSo = langSo.ToArray();
        }

        public static string[] GetLanguagesStrings()
        {
            if(_languagesSo == null)
                RefreshLanguages();
            return _languagesSo.ToList().Select(x => x.languageKey).ToArray();
        }

        public static LanguageSo GetBank()
        {
            if (_actualLanguageBank == null || _actualLanguageBank.languageKey != ActualLanguageKey)
            {
                _actualLanguageBank = _languagesSo.FirstOrDefault(x=> x.languageKey == ActualLanguageKey);
            }
            return _actualLanguageBank;
        }

        public static void AddKey(string key)
        {
            _languagesSo.ForEach(x => x.LanguageDict.TryAdd(key, "TODO"));
        }

        public static void SaveJson()
        {
            FileStream stream = new FileStream(Application.persistentDataPath + "/loca.json",
                File.Exists(Application.persistentDataPath + "/loca.json") ? FileMode.Truncate : FileMode.Create);
            using (StreamWriter writer = new StreamWriter(stream))
            {
                var data = "{\"loca\":{";
                _languagesSo.ForEach(x =>
                {
                    data += "\"" + x.languageKey + "\":{ ";
                    x.LanguageDict.ForEach(y =>
                    {
                        data += "\"" + y.Key + "\":\"" + y.Value + "\",";
                    });
                    data += "},";
                });
                data += "}}";
                writer.Write(data);
            }
        }
        
        public static void LoadJson()
        {
            FileStream stream = new FileStream(Application.persistentDataPath + "/loca.json", FileMode.OpenOrCreate);
            using (StreamReader reader = new StreamReader(stream))
            {
                var a = reader.ReadToEnd();
                var test = a.Split("{");
                
            }
        }
    }
}
