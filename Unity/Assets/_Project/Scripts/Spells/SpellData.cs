using Project.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Spells
{
    [CreateAssetMenu(fileName = "New SpellData", menuName = "Spells/Data/Default", order = 1)]
    public class SpellData : ScriptableObject
    {        
        public const int CharacterSpellsCount = 4;

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

        public float cooldown;
    }
}