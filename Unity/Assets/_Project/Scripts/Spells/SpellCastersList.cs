using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Project._Project.Scripts.Spells
{
    [CreateAssetMenu(fileName = "SpellCastersList", menuName = "Spells/SpellCasters List", order = 0)]
    public class SpellCastersList : ScriptableObject
    {
        [SerializedDictionary("Type", "Caster")] 
        public SerializedDictionary<CastResultType, SpellCaster> spellCasterPrefabs;

        public SpellCaster Get(CastResultType type)
        {
            return !spellCasterPrefabs.ContainsKey(type) ? null : spellCasterPrefabs[type];
        }
    }
}