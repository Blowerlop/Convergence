using System.Linq;
using Project.Extensions;
using Project.Spells.Casters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Spells
{
    [CreateAssetMenu(fileName = "New SpellData", menuName = "Spells/Data/Default", order = 1)]
    public class SpellData : ScriptableObject, IScriptableObjectSerializeReference
    {        
        public const int CharacterSpellsCount = 4;

        [OnValueChanged("UpdateHash")] public string spellId;
        [DisableIf("@true")] public int spellIdHash;
        
        [BoxGroup("Caster")] public SpellCaster requiredCaster;
        [BoxGroup("Caster")] public CastResultType requiredResultType;
        
        [BoxGroup("Spell")] public Spell spellPrefab;
        [BoxGroup("Spell")] public float cooldown;
        [BoxGroup("Spell")] public float channelingTime;

        [BoxGroup("Spell")] public int baseDamage;
        
        void Awake()
        {
            UpdateHash();
        }
        
        private void UpdateHash()
        {
            spellIdHash = spellId.ToHashIsSameAlgoOnUnreal();
        }

        public static SpellData GetSpell(int spellIdHash)
        {
            var spells = SOScriptableObjectReferencesCache.GetScriptableObjects<SpellData>();
            
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
            
            return spells.FirstOrDefault(spell => spell.spellIdHash == spellIdHash);
        }
    }
}