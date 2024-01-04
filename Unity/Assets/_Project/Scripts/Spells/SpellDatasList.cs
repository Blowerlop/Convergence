using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Spells
{
    [CreateAssetMenu(fileName = "SpellDatasList", menuName = "Spells/SpellDatas List", order = 0)]
    public class SpellDatasList : SerializedScriptableObject
    {
        [SerializeField] private SpellData[] spells;

        private Dictionary<int, SpellData> _spellsDict;
        
        public SpellData Get(int hash)
        {
            if(_spellsDict == null) _spellsDict = spells.ToDictionary(data => data.HashedID);
            
            return !_spellsDict.ContainsKey(hash) ? null : _spellsDict[hash];
        }
    }
}