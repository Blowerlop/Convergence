using UnityEngine;

namespace Project._Project.Scripts.UI.Settings
{
    public class AudioSettingsUI : MonoBehaviour
    {
        [SerializeField] private SliderExtended _masterSlider;
        [SerializeField] private SliderExtended _musicSlider;
        [SerializeField] private SliderExtended _gameSoundsSlider;


        private void Start()
        {
            RefreshSliders();
        }

        private void OnEnable()
        {
            RefreshSliders();
                
            _masterSlider.onPointerUp.AddListener(OnPointerUp_SetMaster);
            _musicSlider.onPointerUp.AddListener(OnPointerUp_SetMusic);
            _gameSoundsSlider.onPointerUp.AddListener(OnPointerUp_SetGameSounds);
        }

        private void OnDisable()
        {
            _masterSlider.onPointerUp.RemoveListener(OnPointerUp_SetMaster);
            _musicSlider.onPointerUp.RemoveListener(OnPointerUp_SetMusic);
            _gameSoundsSlider.onPointerUp.RemoveListener(OnPointerUp_SetGameSounds);
        }


        private void RefreshSliders()
        {
            _masterSlider.SetValueWithoutNotify(Get(AudioSettingsManager.KEY_MASTER));
            _musicSlider.SetValueWithoutNotify(Get(AudioSettingsManager.KEY_MUSIC));
            _gameSoundsSlider.SetValueWithoutNotify(Get(AudioSettingsManager.KEY_GAME_SOUNDS));
        }


        private float Get(string exposedVolumeName)
        {
            return AudioSettingsManager.Get01(exposedVolumeName) * 100.0f;
        }

        private void Set(string exposedVolumeName, float value)
        {
            float value01 = value / 100.0f;
            
            AudioSettingsManager.Set(exposedVolumeName, value01);
        }

        private void OnPointerUp_SetMaster(float value)
        {
            Set(AudioSettingsManager.KEY_MASTER, value);
        }
        
        private void OnPointerUp_SetMusic(float value)
        {
            Set(AudioSettingsManager.KEY_MUSIC, value);
        }
        
        private void OnPointerUp_SetGameSounds(float value)
        {
            Set(AudioSettingsManager.KEY_GAME_SOUNDS, value);
        }
    }
}