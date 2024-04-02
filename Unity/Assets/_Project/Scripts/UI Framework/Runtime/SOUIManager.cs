using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Project.Scripts.UIFramework
{
    public enum EColorType
    {
        Primary,
        Secondary,
        Accent,
        Positive,
        Negative
    }
    
    [CreateAssetMenu(menuName = "Scriptable Objects/UI Manager")]
    public class SOUIManager : ScriptableObject, IScriptableObjectSerializeReference
    {
        [Title("Colors")]
        [ColorUsage(false), OnValueChanged("@UIManager.UpdateUI()")] public Color baseColor = Color.white;
        [ColorUsage(false), OnValueChanged("@UIManager.UpdateUI()")] public Color secondaryColor = Color.white;
        [ColorUsage(false), OnValueChanged("@UIManager.UpdateUI()")] public Color accentColor = Color.white;
        [ColorUsage(false), OnValueChanged("@UIManager.UpdateUI()")] public Color positiveColor = Color.white;
        [ColorUsage(false), OnValueChanged("@UIManager.UpdateUI()")] public Color negativeColor = Color.white;
        
        [Title("Font")]
        public TMP_FontAsset titleFont;
        public TMP_FontAsset contentFont;
        
        [Title("Sounds")]
        public AudioClip hoverSound;
        public AudioClip clickSound;
    }
}
