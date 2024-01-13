using System;
using Project.Spells;
using Project.Spells.Casters;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class PlayerRefs : NetworkBehaviour
    {
        private GRPC_NetworkVariable<int> _assignedTeam = new GRPC_NetworkVariable<int>("AssignedTeam", value: -1);

        [SerializeField] private Transform playerTransform;
        
        [SerializeField] private SpellCastController spellCastController;
        
        [SerializeField] private CooldownController pcCooldowns;
        [SerializeField] private CooldownController mobileCooldowns;
        
        public Transform PlayerTransform => playerTransform;
        
        public static event Action<PlayerRefs> OnLocalPlayerSpawned;
        
        #region Team Linking
        
        // Is done before ServerInit on server
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            _assignedTeam.Initialize();
            
            // OnValueChanged is not called for network object that were already spawned before joining
            // We need to call OnTeamChanged manually, if the value is not the default one
            if(_assignedTeam.Value != -1) OnTeamChanged(-1, _assignedTeam.Value);
            _assignedTeam.OnValueChanged += OnTeamChanged;
            
            spellCastController.Init(this);

            if (IsOwner)
            {
                OnLocalPlayerSpawned?.Invoke(this);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _assignedTeam.OnValueChanged -= OnTeamChanged;
        }

        public void ServerInit(int team)
        {
            if (!IsServer && !IsHost) return;
            
            _assignedTeam.Value = team;
        }

        private void OnTeamChanged(int oldValue, int newValue)
        {
            var oldTeamResult = TeamManager.instance.TryGetTeam(oldValue, out var oldTeam);
            var newTeamResult = TeamManager.instance.TryGetTeam(newValue, out var newTeam);

            if (oldTeamResult)
            {
                UserInstance oldMobile = oldTeam.GetUserInstance(PlayerPlatform.Mobile);
                UserInstance oldPc = oldTeam.GetUserInstance(PlayerPlatform.Pc);
                
                // Do not unlink if another character has already been linked to old team
                if(oldMobile != null && oldMobile.LinkedPlayer == this) oldMobile.UnlinkPlayer();
                if(oldPc != null && oldPc.LinkedPlayer == this) oldPc.UnlinkPlayer();
            }

            if (newTeamResult)
            {
                UserInstance newPc = newTeam.GetUserInstance(PlayerPlatform.Pc);
                UserInstance newMobile = newTeam.GetUserInstance(PlayerPlatform.Mobile);
            
                if (newPc) newPc.LinkPlayer(this);
                if (newMobile) newMobile.LinkPlayer(this);
            }
        }
        
        #endregion
        
        #region Cooldowns

        public CooldownController GetCooldownController(PlayerPlatform platform)
        {
            return platform switch
            {
                PlayerPlatform.Pc => pcCooldowns,
                PlayerPlatform.Mobile => mobileCooldowns,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
            };
        }
        
        #endregion
    }
}