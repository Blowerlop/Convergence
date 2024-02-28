using Sirenix.OdinInspector;
using UnityEngine;


namespace Project
{
    public class PCStats : PlayerStats
    {
        [field: SerializeField] public Health health { get; private set; }
        // + Movement speed / attack speed / damage scale / other stats...
        
        
        [Button]
        public override void ServerInit(SOCharacter character)
        {
            base.ServerInit(character);
            
            health.MaxValue = character.BaseHealth;
            health.Value = character.BaseHealth;
        }
    }
}