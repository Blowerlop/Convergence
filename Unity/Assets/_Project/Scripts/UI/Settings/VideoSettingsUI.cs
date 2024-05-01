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
            // DropdownSelectCurrentResolution();
            // DropdownSelectCurrentDisplayMode();
            
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
        // [SerializeField] private Button _lowQualityButton;
        // [SerializeField] private Button _mediumQualityButton;
        // [SerializeField] private Button _HighButton;
        
        
        // public void Start()
        // {
        //     switch (VideoSettingsManager.quality.currentQualityIndex)
        //     {
        //         case 0:
        //             _lowQualityButton.Select();
        //             break;
        //         
        //         case 1:
        //             _mediumQualityButton.Select();
        //             break;
        //         
        //         case 2:
        //             _HighButton.Select();
        //             break;
        //     }
        // }

        // public void Enable()
        // {
        //     _lowQualityButton.onClick.AddListener(SetLowQuality);
        //     _mediumQualityButton.onClick.AddListener(SetMediumQuality);
        //     _HighButton.onClick.AddListener(SetHighQuality);
        // }
        //
        // public void Disable()
        // {
        //     _lowQualityButton.onClick.RemoveListener(SetLowQuality);
        //     _mediumQualityButton.onClick.RemoveListener(SetMediumQuality);
        //     _HighButton.onClick.RemoveListener(SetHighQuality);
        // }

        
        public void SetQuality(int index)
        {
            switch (index)
            {
                case 1:
                    SetLowQuality();
                    break;
                
                case 2:
                    SetMediumQuality();
                    break;
                
                case 3:
                    SetHighQuality();
                    break;
                
                case 4:
                    SetCustomQuality();
                    break;
                
                default:
                    throw new IndexOutOfRangeException();
            }
        }
        
        public void SetLowQuality() => VideoSettingsManager.quality.SetLowQuality();
        public void SetMediumQuality() => VideoSettingsManager.quality.SetMediumQuality();
        public void SetHighQuality() => VideoSettingsManager.quality.SetHighQuality();
        public void SetCustomQuality() => VideoSettingsManager.quality.SetCustomQuality();
    }

    [Serializable]
    public class FrameRateSettingsUI
    {
        [SerializeField] private TMP_InputField _frameRateInputField;
        [SerializeField] private Button _vSyncButton;


        public void Start()
        {
            _frameRateInputField.SetTextWithoutNotify(VideoSettingsManager.frameRate.ToString());
        }

        public void Enable()
        {
            _frameRateInputField.onDeselect.AddListener(SetSetFrameRate);
            _vSyncButton.onClick.AddListener(ToggleVsync);
        }
        
        public void Disable()
        {
            _frameRateInputField.onDeselect.RemoveListener(SetSetFrameRate);
            _vSyncButton.onClick.RemoveListener(ToggleVsync);
        }
        

        private void SetFrameRate(int value)
        {
            VideoSettingsManager.frameRate.SetFrameRate(value);
        }
        
        private void SetSetFrameRate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("FrameRateSettings inputField text is null or empty. This should never happened");
                return;
            }

            if (int.TryParse(value, out int result))
            {
                SetFrameRate(result);
            }
            else
            {
                Debug.LogError("FrameRateSettings inputField text parse failed. This should never happened");
            }
        }

        private void ToggleVsync()
        {
            VideoSettingsManager.frameRate.SetSync(!VideoSettingsManager.frameRate.GetVSync());
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
            // _qualitySettingsUI.Start();
            _frameRateSettingsUI.Start();
        }
        
        private void OnEnable()
        {
            _resolutionSettingsUISettingsUI.Enable();
            // _qualitySettingsUI.Enable();
            _frameRateSettingsUI.Enable();
        }
        
        private void OnDisable()
        {
            _resolutionSettingsUISettingsUI.Disable();
            // _qualitySettingsUI.Disable();
            _frameRateSettingsUI.Disable();
        }
    }
}