using System;
using System.Collections.Generic;
using System.Linq;
using Project._Project.Scripts.UI.Settings;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    [Serializable]
    public class ResolutionSettingsUI
    {
        public enum ECustomFullScreenModeLabel
        {
            Fullscreen,
            Borderless,
            Windowed
        }
        
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        [SerializeField] private TMP_Dropdown _displayModeDropdown;
        [SerializeField] private Button _applyButton;

        [LabelText("Current Resolution"), ShowInInspector, ReadOnly] private string _currentResolutionName => VideoSettingsManager.resolution.currentResolutionName;
        [ShowInInspector, ReadOnly] private int _selectedResolutionIndex;
        
        [LabelText("Current Resolution"), ShowInInspector, ReadOnly] private string _currentDisplayModeName => VideoSettingsManager.resolution.currentFullScreenMode.ToString();
        [ShowInInspector, ReadOnly] private int _selectedDisplayModeIndex;
        
        public void Start()
        {
            FillResolutionDropDown();
            FillDisplayModeDropDown();
            
            DropdownSelectCurrentResolution();
            DropdownSelectCurrentDisplayMode();
        }

        public void Enable()
        {
            DropdownSelectCurrentResolution();
            DropdownSelectCurrentDisplayMode();
            
            _resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownValueChanged_CacheIndex);
            _displayModeDropdown.onValueChanged.AddListener(OnDisplayModeDropdownValueChanged_CacheIndex);
            
            _applyButton.onClick.AddListener(ApplyResolution);
        }
        
        public void Disable()
        {
            _resolutionDropdown.onValueChanged.RemoveListener(OnResolutionDropdownValueChanged_CacheIndex);
            _displayModeDropdown.onValueChanged.RemoveListener(OnDisplayModeDropdownValueChanged_CacheIndex);
            
            _applyButton.onClick.RemoveListener(ApplyResolution);
        }


        private void SetResolution(int width, int height) => SetResolution(width, height, VideoSettingsManager.resolution.currentFullScreenMode);
        private void SetResolution(int width, int height, FullScreenMode fullScreenMode)
        {
            VideoSettingsManager.resolution.SetResolution(width, height, fullScreenMode);
        }

        private void FillResolutionDropDown()
        {
            var resolutionsName = Screen.resolutions.Select(resolution => $"{resolution.width} x {resolution.height}").Distinct().Reverse().ToList();
            _resolutionDropdown.AddOptions(resolutionsName);
        }
        
        private void FillDisplayModeDropDown()
        {
            List<string> fullScreenModeNames = Enum.GetNames(typeof(ECustomFullScreenModeLabel)).ToList();
            _displayModeDropdown.AddOptions(fullScreenModeNames); 
        }

        private void DropdownSelectCurrentResolution()
        {
            int index = _resolutionDropdown.options.FindIndex(optionData => optionData.text == _currentResolutionName);
            _resolutionDropdown.value = index;
        }
        
        private void DropdownSelectCurrentDisplayMode()
        {
            int index = _displayModeDropdown.options.FindIndex(optionData => optionData.text == _currentDisplayModeName);
            _displayModeDropdown.value = index;
        }

        private void OnResolutionDropdownValueChanged_CacheIndex(int index)
        {
            _selectedResolutionIndex = index;
        }
        
        private void OnDisplayModeDropdownValueChanged_CacheIndex(int index)
        {
            _selectedDisplayModeIndex = index;
        }

        private void ApplyResolution()
        {
            Resolution resolution = ExtractResolutionFromOption(_resolutionDropdown.options[_selectedResolutionIndex]);

            ECustomFullScreenModeLabel customFullScreenModeLabel = (ECustomFullScreenModeLabel)_selectedDisplayModeIndex;
            FullScreenMode fullScreenMode = ConvertCustomFullScreenModeToUnityOne(customFullScreenModeLabel);
            
            SetResolution(resolution.width, resolution.height, fullScreenMode);
        }

        private Resolution ExtractResolutionFromOption(TMP_Dropdown.OptionData optionData)
        {
            string[] split = optionData.text.Split('x', StringSplitOptions.RemoveEmptyEntries);
            return new Resolution{width = int.Parse(split[0]), height = int.Parse(split[1])};
        }

        private FullScreenMode ConvertCustomFullScreenModeToUnityOne(
            ECustomFullScreenModeLabel customFullScreenModeLabel)
        {
            return customFullScreenModeLabel switch
            {
                ECustomFullScreenModeLabel.Fullscreen => FullScreenMode.ExclusiveFullScreen,
                ECustomFullScreenModeLabel.Borderless => FullScreenMode.FullScreenWindow,
                ECustomFullScreenModeLabel.Windowed => FullScreenMode.Windowed,
                _ => throw new ArgumentOutOfRangeException(nameof(customFullScreenModeLabel), customFullScreenModeLabel,
                    null)
            };
        }
    }
    
    [Serializable]
    public class QualitySettingsUI
    {
        [SerializeField] private Button _lowQualityButton;
        [SerializeField] private Button _mediumQualityButton;
        [SerializeField] private Button _HighButton;


        public void Start()
        {
            switch (VideoSettingsManager.quality.currentQualityIndex)
            {
                case 0:
                    _lowQualityButton.Select();
                    break;
                
                case 1:
                    _mediumQualityButton.Select();
                    break;
                
                case 2:
                    _HighButton.Select();
                    break;
            }
        }

        public void Enable()
        {
            _lowQualityButton.onClick.AddListener(SetLowQuality);
            _mediumQualityButton.onClick.AddListener(SetMediumQuality);
            _HighButton.onClick.AddListener(SetHighQuality);
        }
        
        public void Disable()
        {
            _lowQualityButton.onClick.RemoveListener(SetLowQuality);
            _mediumQualityButton.onClick.RemoveListener(SetMediumQuality);
            _HighButton.onClick.RemoveListener(SetHighQuality);
        }


        private void SetLowQuality() => VideoSettingsManager.quality.SetLowQuality();
        private void SetMediumQuality() => VideoSettingsManager.quality.SetMediumQuality();
        private void SetHighQuality() => VideoSettingsManager.quality.SetHighQuality();
        private void SetCustomQuality() => VideoSettingsManager.quality.SetCustomQuality();
    }

    [Serializable]
    public class FrameRateSettingsUI
    {
        [SerializeField] private TMP_InputField _inputField;


        public void Start()
        {
            _inputField.SetTextWithoutNotify(VideoSettingsManager.frameRate.ToString());
        }

        public void Enable()
        {
            _inputField.onDeselect.AddListener(Set);
        }
        
        public void Disable()
        {
            _inputField.onDeselect.RemoveListener(Set);
        }
        

        private void Set(int value)
        {
            VideoSettingsManager.frameRate.Set(value);
        }
        
        private void Set(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("FrameRateSettings inputField text is null or empty. This should never happened");
                return;
            }

            if (int.TryParse(value, out int result))
            {
                Set(result);
            }
            else
            {
                Debug.LogError("FrameRateSettings inputField text parse failed. This should never happened");
            }
        }
    }


    public class VideoSettingsUI : MonoBehaviour
    {
        [SerializeField] private ResolutionSettingsUI _resolutionSettingsUISettingsUI = new ResolutionSettingsUI();
        [SerializeField] private FrameRateSettingsUI _frameRateSettingsUI = new FrameRateSettingsUI();
        [SerializeField] private QualitySettingsUI _qualitySettingsUI = new QualitySettingsUI();
        
        
        private void Start()
        {
            _resolutionSettingsUISettingsUI.Start();
            _qualitySettingsUI.Start();
            _frameRateSettingsUI.Start();
        }
        
        private void OnEnable()
        {
            _resolutionSettingsUISettingsUI.Enable();
            _qualitySettingsUI.Enable();
            _frameRateSettingsUI.Enable();
        }
        
        private void OnDisable()
        {
            _resolutionSettingsUISettingsUI.Disable();
            _qualitySettingsUI.Disable();
            _frameRateSettingsUI.Disable();
        }
    }
}