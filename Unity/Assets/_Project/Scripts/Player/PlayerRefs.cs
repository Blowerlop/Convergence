using Project._Project.Scripts;
using Project._Project.Scripts.Player.States;
using Project._Project.Scripts.Spells;
using Project.Spells;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class PlayerRefs : NetworkBehaviour
    {
        private GRPC_NetworkVariable<int> _assignedTeam = new GRPC_NetworkVariable<int>("AssignedTeam", value: -1);
        private GRPC_NetworkVariable<int> _ownerId = new GRPC_NetworkVariable<int>("OwnerId", value: int.MaxValue);

        [SerializeField] protected Transform playerTransform;

        [SerializeField] private CooldownController cooldowns;
        [SerializeField] private ChannelingController channeling;
        
        public int TeamIndex => _assignedTeam.Value;
        public int OwnerId => _ownerId.Value;
        
        public Transform PlayerTransform => playerTransform;
        
        public CooldownController Cooldowns => cooldowns;
        public ChannelingController Channeling => channeling;

        #region Team Linking
        
        // Is done before ServerInit on server
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            _assignedTeam.Initialize();
            _ownerId.Initialize();
            
            // OnValueChanged is not called for network object that were already spawned before joining
            // We need to call OnTeamChanged manually, if the value is not the default one
            if(_assignedTeam.Value != -1) OnTeamChanged(-1, _assignedTeam.Value);
            _assignedTeam.OnValueChanged += OnTeamChanged;
            
            if(_ownerId.Value != int.MaxValue) OnOwnerChanged(int.MaxValue, _ownerId.Value);
            _ownerId.OnValueChanged += OnOwnerChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _assignedTeam.OnValueChanged -= OnTeamChanged;
            _ownerId.OnValueChanged -= OnOwnerChanged;
        }

        [Server]
        public virtual void ServerInit(int team, int ownerId, SOEntity entity)
        {
            _ownerId.Value = ownerId;
            _assignedTeam.Value = team;
        }

        protected virtual void OnTeamChanged(int oldValue, int newValue) { }
        
        protected virtual void OnOwnerChanged(int oldId, int newId)
        {
            if(UserInstanceManager.instance.TryGetUserInstance(oldId, out var oldUser))
            {
                oldUser.UnlinkPlayer();
            }
            
            if(UserInstanceManager.instance.TryGetUserInstance(newId, out var newUser))
            {
                newUser.LinkPlayer(this);
            }
        }
        
        #endregion
    }
}