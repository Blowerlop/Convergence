using Project._Project.Scripts;
using Project._Project.Scripts.StateMachine;
using Project.Spells;
using Project.Spells.Casters;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Project
{
    public class PCPlayerRefs : PlayerRefs
    {
        [SerializeField] private Entity _entity;
        
        [SerializeField] private StateMachineController _stateMachine;
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private PlayerMouse _playerMouse;
        [SerializeField] private MovementController _movementController;
        [SerializeField] private SpellCastController spellCastController;
        
        [SerializeField] private InCastController inCastController;
        
        [SerializeField] private EmoteController emoteController;
        
        public Entity Entity => _entity;
        public StateMachineController StateMachine => _stateMachine;
        public NavMeshAgent NavMeshAgent => _navMeshAgent;
        public PlayerMouse PlayerMouse => _playerMouse;
        public MovementController MovementCOntroller => _movementController;
        
        public InCastController InCastController => inCastController;
        
        public EmoteController EmoteController => emoteController;
        
        [Server]
        public override void ServerInit(int team, int ownerId, SOEntity entity)
        {
            base.ServerInit(team, ownerId, entity);
            
            _entity.ServerInit(entity);
        }
        
        protected override void OnOwnerChanged(int oldId, int newId)
        {
            base.OnOwnerChanged(oldId, newId);
            
            spellCastController.Init(this);
        }

        protected override void OnTeamChanged(int oldValue, int newValue)
        {
            base.OnTeamChanged(oldValue, newValue);
            
            // Maybe change color or something idk
        }
    }
}