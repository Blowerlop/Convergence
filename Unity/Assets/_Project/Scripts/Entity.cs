using Unity.Netcode;
using UnityEngine;

namespace Project._Project.Scripts
{
    public class Entity : NetworkBehaviour, IHealable, IDamageable/*, ITargetable*/
    {
        [SerializeField] private PCStats _stats;
        public PCStats Stats => _stats;

        public virtual int TeamIndex => -1;
        
        
        [Server]
        public virtual void ServerInit(int team, int ownerId, SOCharacter character)
        {
            _stats.ServerInit(character);
        }

        public void Heal(int modifier)
        {
            _stats.health.Value += modifier;
        }

        public void MaxHeal()
        {
            _stats.health.SetToMaxValue();
        }

        public void Damage(int modifier)
        {
            _stats.health.Value -= modifier;
        }

        public bool CanDamage(int teamIndex)
        {
            return TeamIndex != teamIndex;
        }
    }
}