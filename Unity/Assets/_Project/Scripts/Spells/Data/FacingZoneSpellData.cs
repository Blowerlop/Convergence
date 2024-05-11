using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Spells
{
    [CreateAssetMenu(fileName = "New FacingZoneData", menuName = "Spells/Data/Facing Zone", order = 1)]
    public class FacingZoneSpellData : SpellData
    {
        [BoxGroup("Facing Zone Data")] public float limitRadius;
        [BoxGroup("Facing Zone Data")] public float zoneSize;
    }
}