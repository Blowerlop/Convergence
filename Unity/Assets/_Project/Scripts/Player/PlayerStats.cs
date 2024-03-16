using System;
using System.Collections.Generic;
using Project.Extensions;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class PlayerStats : NetworkBehaviour
    {
        [SerializeField] private PlayerRefs playerRefs;
        public PlayerRefs PlayerRefs => playerRefs;
        
        [ShowInInspector, ListDrawerSettings(IsReadOnly = true)] private Dictionary<Type, StatBase> _stats = new Dictionary<Type, StatBase>();
        [field: SerializeReference] public NetworkHealth nHealthStat { get; private set; }
        
        public event Action OnStatsInitialized;
        
        [Server]
        public void ServerInit(SOEntity entity)
        {
            SOCharacter character = (SOCharacter)entity;
            
            character.stats.ForEach(stat =>
            {
                _stats.Add(stat.GetType(), (StatBase)stat.Clone());
            });
            
            nHealthStat.Init();
            
            OnStatsInitialized?.Invoke();
        }
        
        public T Get<T>() where T : StatBase
        {
            return (T)_stats[typeof(T)];
        }
    }
}