using Project.Extensions;
using Project.Spells.Casters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Spells
{
    [CreateAssetMenu(fileName = "New SpellData", menuName = "Spells/Data/Default", order = 1)]
    public class SpellData : ScriptableObject
    {        
        public const int CharacterSpellsCount = 4;

        public string spellId;
        
        [BoxGroup("Caster")] public SpellCaster requiredCaster;
        [BoxGroup("Caster")] public CastResultType requiredResultType;
        
        [BoxGroup("Spell")] public Spell spellPrefab;
        [BoxGroup("Spell")] public float cooldown;
        [BoxGroup("Spell")] public float channelingTime;
    }
}