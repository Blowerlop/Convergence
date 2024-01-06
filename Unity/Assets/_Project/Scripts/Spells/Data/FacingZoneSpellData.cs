using UnityEngine;

namespace Project.Spells
{
    [CreateAssetMenu(fileName = "New FacingZoneData", menuName = "Spells/Data/Facing Zone", order = 1)]
    public class FacingZoneSpellData : SpellData
    {
        public float limitRadius;
        public float zoneSize;
    }
}