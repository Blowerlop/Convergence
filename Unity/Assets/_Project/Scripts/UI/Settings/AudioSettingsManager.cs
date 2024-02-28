using System.Threading.Tasks;
using DG.Tweening;
using Project._Project.Scripts.Managers;
using UnityEngine;
using UnityEngine.Audio;

namespace Project._Project.Scripts.UI.Settings
{
    public static class AudioSettingsManager
    {
        private static AudioMixer _audioMixer;
        private static AudioMixer audioMixer
        {
            get
            {
                if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer");

                return _audioMixer;
            }
        }

        public const string KEY_MASTER = "MasterBus";
        public const string KEY_MUSIC = "Music";
        public const string KEY_SFX = "SFX";
        
        private static readonly string _logPrefix = "[AudioManager]";
        
        
        private static float GetDb(string exposedVolumeName) => PlayerPrefs.GetFloat(exposedVolumeName, Convert01ToDb(0.5f));
        public static float Get01(string exposedVolumeName) => ConvertDbTo01(GetDb(exposedVolumeName));

        public static void Set(string exposedVolumeName, float value01)
        {
            float realVolume = Convert01ToDb(value01);
            
            SetWithoutNotify(exposedVolumeName, value01);
            OnSave(exposedVolumeName, realVolume);
        }

        private static void SetWithoutNotify(string exposedVolumeName, float value01)
        {
            float realVolume = Convert01ToDb(value01);
            audioMixer.SetFloat(exposedVolumeName, realVolume);
            SoundManager.instance.SetBusVolume(value01, exposedVolumeName);
        }

        private static void OnSave(string exposedVolumeName, float realVolume)
        {
            Save(exposedVolumeName, realVolume);
        }

        private static void Save(string exposedVolumeName, float volume)
        {
            PlayerPrefs.SetFloat(exposedVolumeName, volume);
            PlayerPrefs.Save();
            
            Debug.Log($"{_logPrefix} Saved : {exposedVolumeName} ({volume}dB)");
        }
        
        private static float Convert01ToDb(float value01)
        {
            return Mathf.Log10(Mathf.Clamp(value01, 0.0001f, 1f)) * (20.0f);
        }

        private static float ConvertDbTo01(float volume)
        {
            return Mathf.Round(Mathf.Pow(10, volume / 20) * 100) / 100;
        }
        
        public static async void Load()
        {
            await Task.Delay(100);
            
            SetWithoutNotify(KEY_MASTER, Get01(KEY_MASTER));
            SetWithoutNotify(KEY_MUSIC,Get01(KEY_MUSIC));
            SetWithoutNotify(KEY_SFX, Get01(KEY_SFX));
            
            Debug.Log($"{_logPrefix} Volumes loaded : \n " +
                      $"- {KEY_MASTER} {GetDb(KEY_MASTER)} dB \n " +
                      $"- {KEY_MUSIC} {GetDb(KEY_MUSIC)} dB \n " +
                      $"- {KEY_SFX} {GetDb(KEY_SFX)} dB");
        }
        
        
        #region Commands

        [ConsoleCommand("volume", "Set the master volume")]
        private static void SetMasterVolume(float value01) => Set(KEY_MASTER, value01);
        
        [ConsoleCommand("volume_m", "Set the music volume")]
        private static void SetMusicVolume(float value01) => Set(KEY_MUSIC, value01);
        
        [ConsoleCommand("volume_fx", "Set the game sounds volume")]
        private static void SetGameSoundsVolume(float value01) => Set(KEY_MASTER, value01);
        #endregion
    }
}