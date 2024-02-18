using Project._Project.Scripts.Player.States;
using UnityEngine;

namespace Project
{
    public class PlayerLifeTime : MonoBehaviour
    {
        private PlayerStateMachineController _stateMachine;
        private PCStats _stats;


        private void Awake()
        {
            PlayerRefs playerRefs = GetComponentInParent<PlayerRefs>();
            _stats = (PCStats)playerRefs.Stats;
            _stateMachine = playerRefs.StateMachine;
        }

        private void Start()
        {
            _stats.OnHealthChanged += OnHealthChanged_CheckIfDead;
        }

        
        private void OnHealthChanged_CheckIfDead(int currentHealth, int maxHealth)
        {
            if (currentHealth <= 0) _stateMachine.ChangeState(_stateMachine.deadState);
        }
    }
}
