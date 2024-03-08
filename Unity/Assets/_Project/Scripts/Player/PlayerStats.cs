using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class PlayerStats : NetworkBehaviour
    {
        [SerializeField] private PlayerRefs playerRefs;
        public PlayerRefs PlayerRefs => playerRefs;
        
        [field: SerializeReference] public Health health { get; private set; }
        public readonly AttackDamage attackDamage = new AttackDamage();
        public readonly AttackSpeed attackSpeed = new AttackSpeed();
        public readonly AttackRange attackRange = new AttackRange();

        [Server]
        public void ServerInit(SOEntity entity)
        {
            SOCharacter character = (SOCharacter)entity;
            
            health.MaxValue = character.BaseHealth;
            health.Value = character.BaseHealth;
            
            attackDamage.MaxValue = character.BaseAttackDamage;
            attackDamage.Value = character.BaseAttackDamage;

            attackSpeed.MaxValue = character.BaseAttackSpeed;
            attackSpeed.Value = character.BaseAttackSpeed;
            
            attackRange.MaxValue = character.BaseAttackRange;
            attackRange.Value = character.BaseAttackRange;
        }
    }
}