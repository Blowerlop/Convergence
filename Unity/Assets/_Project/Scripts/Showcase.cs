using System;
using Project.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    public class Showcase : MonoBehaviour
    {
        [SerializeField] private SOCharacter _characterData;

        [Title("References")] 
        [SerializeField] private TMP_Text _name;
        [SerializeField] private Image _spell1;
        [SerializeField] private Image _spell2;
        [SerializeField] private Image _spell3;
        [SerializeField] private Image _spell4;
        [SerializeField] private GameObject _previewSpawnPoint;


        private void OnValidate()
        {
            if (_characterData == null) return;
            
            _name.text = _characterData.characterName;
            
            var spells = _characterData.GetSpells();
            _spell1.sprite = spells[0].spellIcon;
            _spell2.sprite = spells[1].spellIcon;
            _spell3.sprite = spells[2].spellIcon;
            _spell4.sprite = spells[3].spellIcon;
            
            
            UnityEditor.EditorApplication.delayCall+=()=>
            {
                _previewSpawnPoint.DestroyChildren();
                var modelInstance = Instantiate(_characterData.model, Vector3.zero, Quaternion.identity, _previewSpawnPoint.transform);
                modelInstance.GetComponentsInChildren<Transform>().ForEach(x => x.gameObject.layer = Constants.Layers.EntityIndex); 
            };
        }
    }
}
