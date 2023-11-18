using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project._Project.Scripts.Spells
{
    [CreateAssetMenu(fileName = "SpellCastersList", menuName = "Spells/SpellCasters List", order = 0)]
    public class SpellCastersList : SerializedScriptableObject
    {
        public Dictionary<CastResultType, SpellCaster> spellCasterPrefabs;

        public SpellCaster Get(CastResultType type)
        {
            return !spellCasterPrefabs.ContainsKey(type) ? null : spellCasterPrefabs[type];
        }
    }
}