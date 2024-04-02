using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.UIFramework
{
    public enum EComponentTypeUI
    {
        Image,
        Text
    }

    public enum ETextType
    {
        Title,
        Content
    }
    
    
    public sealed class SkinUI : MonoBehaviour
    {
        [SerializeField] private EComponentTypeUI _componentType;
        [SerializeField] private EColorType _colorType;

        [SerializeField, ShowIf("@_componentType == EComponentTypeUI.Text")] private ETextType _textType;
        [SerializeField, ShowIf("@_componentType == EComponentTypeUI.Image")] private bool _useCustomAlpha;


        private void Start()
        {
            OnValidate();
        }

        public void OnValidate()
        {
            switch (_componentType)
            {
                case EComponentTypeUI.Image:
                    Image image = GetComponent<Image>();
                    if (_useCustomAlpha)
                    {
                        Color color = GetColor();
                        color.a = image.color.a;
                        image.color = color;
                    }
                    else image.color = GetColor();
                    break;
                
                case EComponentTypeUI.Text:
                    TMP_Text text = GetComponent<TMP_Text>();
                    text.color = GetColor();
                    text.font = GetFont();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
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