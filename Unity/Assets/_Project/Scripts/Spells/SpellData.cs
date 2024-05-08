using System;
using System.Linq;
using Project.Extensions;
using Project.Spells.Casters;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Project.Spells
{
    [CreateAssetMenu(fileName = "New SpellData", menuName = "Spells/Data/Default", order = 1)]
    public class SpellData : ScriptableObject, IScriptableObjectSerializeReference
    {        
        public const int CharacterSpellsCount = 4;

        // Ugly but working
        // Find a way to get rid of magic string
        [BoxGroup("Caster"), Required, SerializeField, 
         ValueDropdown("@ICastResult.AllResultTypesAsString"), OnValueChanged(nameof(CheckCasterValidity))]
        private string resultTypeSelection;
        
        [BoxGroup("Caster"), OnValueChanged(nameof(CheckCasterValidity))] 
        public SpellCaster requiredCaster;
        
        [BoxGroup("Spell"), OnValueChanged("UpdateHash")] public string spellId;
        [BoxGroup("Spell"), DisableIf("@true")] public int spellIdHash;
        [Space(15)]
        [BoxGroup("Spell"), InlineEditor] public Spell spellPrefab;
        [Space(15)]
        [BoxGroup("Spell")] public bool isInstant;
        [BoxGroup("Spell")] public float cooldown;
        [BoxGroup("Spell")] public float channelingTime;

        [Space(40)]
        
        [SerializeReference, PropertyOrder(999)] public Effect[] effects;

        public Type RequiredResultType => resultTypeSelection != null ? Type.GetType(resultTypeSelection) : null;
        
        [InfoBox("Replace casting animation with the animation of the spell")]
        [BoxGroup("Spell")] public AnimatorOverrideController animatorOverrideController;
        
        void Awake()
        {
            UpdateHash();
        }
        
        private void UpdateHash()
        {
            if (spellId == null) return;
            
            spellIdHash = spellId.ToHashIsSameAlgoOnUnreal();
        }

        public static SpellData GetSpell(int spellIdHash)
        {
            // TODO: Fix this shit
            var spells = SOScriptableObjectReferencesCache.GetScriptableObjects<SpellData>();
            var spells2 = SOScriptableObjectReferencesCache.GetScriptableObjects<ZoneSpellData>();
            var spells3 = SOScriptableObjectReferencesCache.GetScriptableObjects<FacingZoneSpellData>();
            
            Debug.Log("> Get spell with hash: " + spellIdHash + ": ");
            
            foreach (var spellData in spells)
            {
                if (spellData == null)
                {
                    Debug.Log("Spell data null ???");
                    continue;
                }
                
                Debug.Log(spellData.spellIdHash + " - " + spellData.spellId);
            }
            
            // TODO: Fix this shit aussi
            var s = spells.FirstOrDefault(spell => spell.spellIdHash == spellIdHash);
            if (s == null)
            {
                s = spells2.FirstOrDefault(spell => spell.spellIdHash == spellIdHash);
            }
            if (s == null)
            {
                s = spells3.FirstOrDefault(spell => spell.spellIdHash == spellIdHash);
            }
            
            return s;
        }
        
        #if UNITY_EDITOR

        [PropertySpace, Button(ButtonSizes.Large, ButtonStyle.Box, ButtonHeight = 30, Name = "Force Link To Prefab")]
        private void ForceLinkToPrefab()
        {
            if (spellPrefab == null) return;
            
            GameObject contentsRoot = spellPrefab.gameObject;

            spellPrefab.Data = this;

            PrefabUtility.SavePrefabAsset(contentsRoot);
            
            Debug.Log($"<color=lime>Linked</color>");
        }
        #endif

        #region Utilities
        
        private void CheckCasterValidity()
        {
            if (requiredCaster == null) return;

            if (resultTypeSelection == null)
            {
                Debug.LogError("You need to define <b>resultTypeSelection</b> before defining a caster!");
                
                requiredCaster = null;
                return;
            }

            if (RequiredResultType != requiredCaster.CastResultType)
            {
                Debug.LogError($"Selected caster <color=red>{requiredCaster.gameObject.name}</color> has a different CastResultType " +
                               $"than the one required by this spell! Required: <color=red>{RequiredResultType}</color>, Caster: " +
                               $"<color=red>{requiredCaster.CastResultType}</color>");
                
                requiredCaster = null;
            }
        }
        
        #endregion
    }
}