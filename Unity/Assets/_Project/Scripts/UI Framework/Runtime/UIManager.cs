using System;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Project.Scripts.UIFramework
{
    public static class UIManager
    {
        public static SOUIManager instance;
        

        static UIManager()
        {
            instance = SOScriptableObjectReferencesCache.GetScriptableObjects<SOUIManager>()[0];
        }

        
        public static void UpdateUI()
        {
            Object.FindObjectsOfType<SkinUI>().ForEach(x => x.OnValidate());
        }

        public static Color GetColorByType(EColorType type)
        {
            return type switch
            {
                EColorType.Primary => instance.baseColor,
                EColorType.Secondary => instance.secondaryColor,
                EColorType.Accent => instance.accentColor,
                EColorType.Positive => instance.positiveColor,
                EColorType.Negative => instance.negativeColor,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        public static TMP_FontAsset GetFontByType(ETextType type)
        {
            return type switch
            {
                ETextType.Title => instance.titleFont,
                ETextType.Content => instance.contentFont,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
