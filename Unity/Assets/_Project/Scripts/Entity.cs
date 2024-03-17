using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project._Project.Scripts
{
    public abstract class Entity : NetworkBehaviour, IHealable, IDamageable, IShieldable/*, ITargetable*/
    {
        [field: ShowInInspector, ReadOnly, ServerField] public SOEntity data { get; private set; }
        [SerializeField] protected PlayerStats _stats;
        public PlayerStats Stats => _stats;

        public virtual int TeamIndex => -1;

        private bool _isInit;
        private event Action _onEntityInit;
        public event Action onEntityInit
        {
            add
            {
                if (_isInit)
                {
                    value.Invoke();
                }
                else _onEntityInit += value;
            }
            remove => _onEntityInit -= value;
        }

        [Server]
        public void ServerInit(SOEntity entityData)
        {
            data = entityData;
            _stats.ServerInit(entityData);
            
            _onEntityInit?.Invoke();
            _onEntityInit = null;
            _isInit = true;
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
        
        public int Shield(int modifier)
        {
            var shield = new ShieldData(modifier);
            
            _stats.nShieldStat.AddShield(shield);
            return shield.ID;
        }

        public void UnShield(int shieldId)
        {
            _stats.nShieldStat.RemoveShield(shieldId);
        }
    }
}