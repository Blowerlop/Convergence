using Sirenix.OdinInspector;
using UnityEngine;

namespace Project._Project.Scripts.Spells
{
    [CreateAssetMenu(fileName = "New SpellData", menuName = "Spells/Spell Data", order = 1)]
    public class SpellData : ScriptableObject
    {
        public string spellId;
        [HideInInspector] public int spellIdHash;
        
        public CastResultType castingType;
        
        public Spell spellPrefab;
        
        [PropertySpace(25)]

        public float baseCooldown;
        public float baseChargeTime;
    }
}