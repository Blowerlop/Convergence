using System;
using System.Collections.Generic;
using System.Linq;
using Michsky.UI.Heat;
using Project._Project.Scripts.UI.Settings;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Dropdown = Michsky.UI.Heat.Dropdown;

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
        
        [SerializeField] private Dropdown _resolutionDropdown;
        [SerializeField] private Dropdown _displayModeDropdown;
        [SerializeField] private ButtonManager _applyButton;

        [LabelText("Current Resolution"), ShowInInspector, ReadOnly] private string _currentResolutionName => VideoSettingsManager.resolution.currentResolutionName;
        [ShowInInspector, ReadOnly] private int _selectedResolutionIndex;
        
        [LabelText("Current Resolution"), ShowInInspector, ReadOnly] private string _currentDisplayModeName => VideoSettingsManager.resolution.currentFullScreenMode.ToString();
        [ShowInInspector, ReadOnly] private int _selectedDisplayModeIndex;
        
        public void Start()
        {
            PopulateResolutionDropDown();
            PopulateDisplayModeDropDown();
            
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

        private void PopulateResolutionDropDown()
        {
            var resolutionsName = Screen.resolutions.Select(resolution => $"{resolution.width} x {resolution.height}").Distinct().Reverse().ToList();
            resolutionsName.ForEach(x => _resolutionDropdown.CreateNewItem(x));
        }
        
        private void PopulateDisplayModeDropDown()
        {
            List<string> fullScreenModeNames = Enum.GetNames(typeof(ECustomFullScreenModeLabel)).ToList();
            fullScreenModeNames.ForEach(x => _displayModeDropdown.CreateNewItem(x));
        }

        private void DropdownSelectCurrentResolution()
        {
            int index = _resolutionDropdown.items.FindIndex(optionData => optionData.itemName == _currentResolutionName);
            _resolutionDropdown.SetDropdownIndex(index);
        }
        
        private void DropdownSelectCurrentDisplayMode()
        {
            int index = _displayModeDropdown.items.FindIndex(optionData => optionData.itemName == _currentDisplayModeName);
            _displayModeDropdown.SetDropdownIndex(index);
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
            Resolution resolution = ExtractResolutionFromOption(_resolutionDropdown.items[_selectedResolutionIndex]);

            ECustomFullScreenModeLabel customFullScreenModeLabel = (ECustomFullScreenModeLabel)_selectedDisplayModeIndex;
            FullScreenMode fullScreenMode = ConvertCustomFullScreenModeToUnityOne(customFullScreenModeLabel);
            
            SetResolution(resolution.width, resolution.height, fullScreenMode);
        }

        private Resolution ExtractResolutionFromOption(Dropdown.Item optionData)
        {
            string[] split = optionData.itemName.Split('x', StringSplitOptions.RemoveEmptyEntries);
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
        [SerializeField] private ButtonManager _lowQualityButton;
        [SerializeField] private ButtonManager _mediumQualityButton;
        [SerializeField] private ButtonManager _HighButton;


        public void Start()
        {
            switch (VideoSettingsManager.quality.currentQualityIndex)
            {
                case 0:
                    // _lowQualityButton.Select();
                    break;
                
                case 1:
                    // _mediumQualityButton.Select();
                    break;
                
                case 2:
                    // _HighButton.Select();
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
        [SerializeField] private InputFieldManager _frameRateInputField;
        [SerializeField] private SwitchManager _vSyncSwitch;


        public void Start()
        {
            _frameRateInputField.inputText.SetTextWithoutNotify(VideoSettingsManager.frameRate.ToString());
            _vSyncSwitch.Set(VideoSettingsManager.frameRate.GetVSync(), true);
        }

        public void Enable()
        {
            _frameRateInputField.inputText.onDeselect.AddListener(SetSetFrameRate);
            _vSyncSwitch.onValueChanged.AddListener(EnableVsync);
        }
        
        public void Disable()
        {
            _frameRateInputField.inputText.onDeselect.RemoveListener(SetSetFrameRate);
            _vSyncSwitch.onValueChanged.RemoveListener(EnableVsync);
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

        private void EnableVsync(bool state)
        {
            VideoSettingsManager.frameRate.SetSync(state);
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