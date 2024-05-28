using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.UIFramework
{
    public enum ETextType
    {
        Title,
        Content
    }
    
    
    public sealed class SkinUI : MonoBehaviour
    {
        #if UNITY_EDITOR
        private enum EComponentTypeUI
        {
            Image,
            Text,
            Null
        }
        [NonSerialized] private EComponentTypeUI _componentType;
        #endif
        [SerializeField] private EColorType _colorType;
        [SerializeField, ShowIf("@_componentType == EComponentTypeUI.Text")] private ETextType _textType;
        [SerializeField, ShowIf("@_componentType == EComponentTypeUI.Image")] private bool _useCustomAlpha;


#if UNITY_EDITOR
        private void Start()
        {
            OnValidate();
        }
        
        public void OnValidate()
        {
            if (TryGetComponent(out Image image))
            {
                _componentType = EComponentTypeUI.Image;
                
                if (_useCustomAlpha)
                {
                    Color color = GetColor();
                    color.a = image.color.a;
                    image.color = color;
                }
                else image.color = GetColor();
            }
            else if (TryGetComponent(out TMP_Text text))
            {
                _componentType = EComponentTypeUI.Text;
                
                text.color = GetColor();
                text.font = GetFont();
            }
            else throw new MissingComponentException("Missing component Image or TMP_Text");
        }
#endif
        
        private Color GetColor()
        {
            return UIManager.GetColorByType(_colorType);
        }

        private TMP_FontAsset GetFont()
        {
            return UIManager.GetFontByType(_textType);
        }
    }
}