using Project.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Spells
{
    [CreateAssetMenu(fileName = "New SpellData", menuName = "Spells/Spell Data", order = 1)]
    public class SpellData : ScriptableObject
    {
        public string spellId;

        private int? hash;
        public int HashedID
        {
            get
            {
                hash ??= spellId.ToLower().ToHashIsSameAlgoOnUnreal();
                return hash.Value;
            }       
        }
        
        public ChannelingResultType castingType;
        
        public Spell spellPrefab;
        
        [PropertySpace(25)]

        public float baseCooldown;
        public float baseChargeTime;
    }
}