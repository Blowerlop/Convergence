﻿using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.UIFramework
{
    public class Button : InteractibleUIElement
    {
        // Icon
        [TabGroup("Group", "Settings")]
        [SerializeField, BoxGroup("Group/Settings/Icon")] private bool _enableIcon = true;
        [SerializeField, BoxGroup("Group/Settings/Icon"), ShowIf("_enableIcon")] private Sprite _icon;
        // Text
        [SerializeField, BoxGroup("Group/Settings/Text")] private bool _enableText = true;
        [SerializeField, BoxGroup("Group/Settings/Text"), ShowIf("_enableText")] private string _text;
        [SerializeField, BoxGroup("Group/Settings/Text"), ShowIf("_enableText")] private bool _textSizeAuto;
        
        // Global settings
        [SerializeField, BoxGroup("Group/Settings/Global settings"), ShowIf("@_enableIcon || _enableText")] private float _size = 24;
        [SerializeField, BoxGroup("Group/Settings/Global settings"), ShowIf("@_enableIcon && _enableText")] private float _space = 10;
        
        // References
        [TabGroup("Group", "References")]
        [SerializeField, BoxGroup("Group/References/Normal")] private CanvasGroup _normal;
        [SerializeField, BoxGroup("Group/References/Normal")] private TMP_Text _normalButtonText;
        [SerializeField, BoxGroup("Group/References/Normal")] private Image _normalButtonIcon;
        [SerializeField, BoxGroup("Group/References/Normal")] private HorizontalLayoutGroup _normalHorizontalLayoutGroup;
        
        [SerializeField, BoxGroup("Group/References/Hover")] private CanvasGroup _hover;


        protected virtual void OnValidate()
        {
            SetupIcon();
            SetupText();
            if (_enableText && _enableIcon)
            {
                _normalHorizontalLayoutGroup.spacing = _space;
            }
        }

        protected virtual void SetupIcon()
        {
            _normalButtonIcon.gameObject.SetActive(_enableIcon);
            
            if (_enableIcon)
            {
                _normalButtonIcon.sprite = _icon;
                LayoutElement buttonIconLayoutElement = _normalButtonIcon.GetComponent<LayoutElement>();
                buttonIconLayoutElement.preferredHeight = _size;
                buttonIconLayoutElement.preferredWidth = _size;
            }
        }

        protected virtual void SetupText()
        {
            _normalButtonText.gameObject.SetActive(_enableText);
            
            if (_enableText)
            {
                _normalButtonText.text = _text;
                if (_textSizeAuto) _normalButtonText.enableAutoSizing = true;
                else
                {
                    _normalButtonText.enableAutoSizing = false;
                    _normalButtonText.fontSize = _size;
                }
            }
        }
    }
}