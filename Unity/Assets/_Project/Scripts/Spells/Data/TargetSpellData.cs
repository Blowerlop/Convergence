using UnityEngine;

namespace Project.Spells
{
    [CreateAssetMenu(fileName = "New TargetData", menuName = "Spells/Data/Target", order = 1)]
    public class TargetSpellData : SpellData
    {
        public float zoneRadius;
    }
}