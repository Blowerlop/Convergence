using Project._Project.Scripts;
using Project._Project.Scripts.Player.States;
using Project.Spells.Casters;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;

namespace Project
{
    public class PCPlayerRefs : PlayerRefs
    {
        [SerializeField] private Entity _entity;
        
        [SerializeField] private PlayerStateMachineController _stateMachine;
        [SerializeField] private NetworkAnimator _networkAnimator;
        [SerializeField] private NavMeshAgent _navMeshAgent;
        
        [SerializeField] private SpellCastController spellCastController;

        [SerializeField] private EmoteController emoteController;
        
        public Entity Entity => _entity;
        public PlayerStateMachineController StateMachine => _stateMachine;
        public Animator Animator => _networkAnimator.Animator;
        public NavMeshAgent NavMeshAgent => _navMeshAgent;

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