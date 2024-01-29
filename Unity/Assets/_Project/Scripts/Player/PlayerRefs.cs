using System;
using Project._Project.Scripts.Spells;
using Project.Spells;
using Project.Spells.Casters;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class PlayerRefs : NetworkBehaviour
    {
        private GRPC_NetworkVariable<int> _assignedTeam = new GRPC_NetworkVariable<int>("AssignedTeam", value: -1);

        [SerializeField] private Transform playerTransform;
        
        // Will be null on mobile PlayerRefs
        [SerializeField] private SpellCastController spellCastController;
        
        [BoxGroup("Cooldowns"),SerializeField] private CooldownController cooldowns;
        [BoxGroup("Channeling"), SerializeField] private ChannelingController channeling;
        [BoxGroup("Stats"), SerializeField] private PlayerStats stats;
        
        public int AssignedTeam => _assignedTeam.Value;
        
        // Find a way to point to PC PlayerRefs on mobile PlayerRefs
        public Transform PlayerTransform => playerTransform;

        public CooldownController Cooldowns => cooldowns;
        public ChannelingController Channeling => channeling;
        public PlayerStats Stats => stats;
        
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
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _assignedTeam.OnValueChanged -= OnTeamChanged;
        }

        [Server]
        public void ServerInit(int team, SOCharacter character)
        {
            _assignedTeam.Value = team;
            stats.ServerInit(character);
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
    }
}