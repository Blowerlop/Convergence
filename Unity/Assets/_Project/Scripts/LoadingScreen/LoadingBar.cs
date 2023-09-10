using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    public class LoadingBar : MonoBehaviour
    {
        [SerializeField, ] private Image _image;
        [SerializeField] private TMP_Text _text;

        
        public void UpdateLoadingBar(float loadingValue)
        {
            if (_image != null)
            {
                _image.fillAmount = loadingValue;
            }

            if (_text != null)
            {
                _text.text = $"{loadingValue.ToString(CultureInfo.InvariantCulture)}%";
            }
        }
    }
}
