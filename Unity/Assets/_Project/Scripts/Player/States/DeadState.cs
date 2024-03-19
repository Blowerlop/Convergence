using Project._Project.Scripts.StateMachine;
using Sirenix.OdinInspector;
using Unity.Netcode.Components;
using UnityEngine;

namespace Project._Project.Scripts.Player.States
{
    public class DeadState : BaseStateMachineBehaviour
    {
        [ShowInInspector] private Timer _deathTimer;
        private Vector3 _position;

        protected override void OnEnter()
        {
            _deathTimer = new Timer();
            _deathTimer.StartTimerWithCallback(playerRefs.StateMachine, GameSettings.instance.deathTime, () =>
            {
                playerRefs.StateMachine.ChangeStateTo<IdleState>();
            });

            _position = playerRefs.PlayerTransform.position;
            playerRefs.PlayerTransform.GetComponent<NetworkTransform>().Teleport(new Vector3(999, 999, 999), Quaternion.identity, Vector3.one);
        }

        protected override void OnExit()
        {
            playerRefs.PlayerTransform.GetComponent<NetworkTransform>().Teleport(_position, Quaternion.identity, Vector3.one);
            playerRefs.Entity.Stats.nHealthStat.SetToMaxValue();
        }

        public override void OnDispose()
        {
            _deathTimer = null;
        }
        
        public override bool CanChangeStateTo<T>()
        {
            return false;
        }

        public override string ToString()
        {
            return "Dead";
        }
    }
}