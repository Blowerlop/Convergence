using Project._Project.TESTT_REBIND;
using Project.Spells;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Project
{
    public class SpellTooltip : MonoBehaviour
    {
        [SerializeField] Image spellImage; 
        [SerializeField] TextMeshProUGUI spellName, spellCooldown, spellDescription;
        public InputActionReference inputActionReference; 

        private void Start()
        {
            gameObject.SetActive(false);
        }
        public void UpdateToolTipText(SpellData spellData)
        {
            spellName.text = "[" + InputSettingsManager.GetBindingName(inputActionReference.action.name, 0) + "] " + spellData.spellName;
            spellCooldown.text = spellData.cooldown.ToString() + "s" ;
            spellDescription.text = spellData.spellDescription.GenerateText();
            spellImage.sprite = spellData.spellIcon; 
        }
    }
}
