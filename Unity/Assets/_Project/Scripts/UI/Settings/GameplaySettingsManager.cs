using Project.Extensions;
using UnityEngine;

namespace Project._Project.Scripts.UI.Settings
{
    [System.Serializable]
    public class GameplaySettings
    {
        public readonly string key;
        private bool _value;
        public bool value
        {
            get => _value;
            set
            {
                _value = value;
                PlayerPrefs.SetInt(key, _value.ToInt());
            }
        }

        
        public GameplaySettings(string key, bool defaultValue)
        {
            this.key = key;
            value = PlayerPrefs.GetInt(key, defaultValue.ToInt()).ToBool();
        }
    }
    
    public static class GameplaySettingsManager
    {
        // Camera
        public static GameplaySettings useMouse = new GameplaySettings("UseMouse", true);
    }   
}
