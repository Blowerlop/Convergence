using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project._Project.Scripts
{
    public abstract class Entity : NetworkBehaviour, IHealable, IDamageable/*, ITargetable*/
    {
        [ShowInInspector, ReadOnly, ServerField] protected SOEntity _data;
        [SerializeField] protected PlayerStats _stats;
        public PlayerStats Stats => _stats;

        public virtual int TeamIndex => -1;


        [Server]
        public void ServerInit(SOEntity entityData)
        {
            _data = entityData;
            _stats.ServerInit(entityData);
        }
        
        public void Heal(int modifier)
        {
            _stats.nHealthStat.Value += modifier;
        }

        public void MaxHeal()
        {
            _stats.nHealthStat.SetToMaxValue();
        }

        public void Damage(int modifier)
        {
            _stats.nHealthStat.Value -= modifier;
        }

        public bool CanDamage(int teamIndex)
        {
            return TeamIndex != teamIndex;
        }
    }
}