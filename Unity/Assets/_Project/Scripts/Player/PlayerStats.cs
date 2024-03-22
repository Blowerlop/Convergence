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
        [field: SerializeReference] public NetworkShield nShieldStat { get; private set; }
        
        public event Action OnStatsInitialized;
        public bool isInitialized;
        
        [Server]
        public void ServerInit(SOEntity entity)
        {
            SOCharacter characterData = (SOCharacter)entity;

            if (IsHost == false)
            {
                InitLocal(characterData);
            }
            
            InitClientRpc(characterData.id);
        }

        [ClientRpc]
        private void InitClientRpc(int characterDataId)
        {
            InitLocal(SOCharacter.GetCharacter(characterDataId));
        }
        
        
        private void InitLocal(SOCharacter characterData)
        {
            characterData.stats.ForEach(stat =>
            {
                _stats.Add(stat.GetType(), (StatBase)stat.Clone());
            });

            nHealthStat.Init();
            nShieldStat.Init();
            
            OnStatsInitialized?.Invoke();
            isInitialized = true;
        }
        
        public T Get<T>() where T : StatBase
        {
            if (!_stats.ContainsKey(typeof(T))) throw new Exception($"Stat of type {typeof(T)} not found");
            
            return (T)_stats[typeof(T)];
        }
        
        public bool TryGet<T>(out T stat) where T : StatBase
        {
            if (!_stats.ContainsKey(typeof(T)))
            {
                stat = default;
                return false;
            }
            
            stat = (T)_stats[typeof(T)];
            return true;
        }
    }
}