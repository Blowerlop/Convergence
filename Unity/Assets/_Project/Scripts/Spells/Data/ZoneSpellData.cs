using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Spells
{
    [CreateAssetMenu(fileName = "New ZoneData", menuName = "Spells/Data/Zone", order = 1)]
    public class ZoneSpellData : SpellData
    {
        [FoldoutGroup("Zone Data")] public float limitRadius;
        [FoldoutGroup("Zone Data")] public float zoneRadius;
    }
}