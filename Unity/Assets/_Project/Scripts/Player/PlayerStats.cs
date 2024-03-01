using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class PlayerStats : NetworkBehaviour
    {
        [SerializeField] private PlayerRefs playerRefs;
        public PlayerRefs PlayerRefs => playerRefs;
        
        [field: SerializeField] public Health health { get; private set; }
        [field: SerializeField] public MoveSpeed moveSpeed { get; private set; }

        [Server]
        public void ServerInit(SOEntity entity)
        {
            SOCharacter character = (SOCharacter)entity;
            
            health.MaxValue = character.BaseHealth;
            health.Value = character.BaseHealth;

            moveSpeed.Value = character.BaseMoveSpeed;
        }
    }
}