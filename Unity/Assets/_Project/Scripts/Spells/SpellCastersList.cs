using System.Collections.Generic;
using Project.Spells.Casters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Spells
{
    [CreateAssetMenu(fileName = "SpellCastersList", menuName = "Spells/SpellCasters List", order = 0)]
    public class SpellCastersList : SerializedScriptableObject
    {
        public Dictionary<ChannelingResultType, SpellCaster> SpellCasterPrefabs = new();

        public SpellCaster Get(ChannelingResultType type)
        {
            return !SpellCasterPrefabs.ContainsKey(type) ? null : SpellCasterPrefabs[type];
        }
    }
}