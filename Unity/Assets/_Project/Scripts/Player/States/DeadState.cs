using Project._Project.Scripts.StateMachine;
using Sirenix.OdinInspector;
using Unity.Netcode.Components;
using UnityEngine;

namespace Project._Project.Scripts.Player.States
{
    public class DeadState : BaseStateMachine
    {
        [ShowInInspector] private Timer _deathTimer;
        private Vector3 _position;
        
        public override void StartState(PlayerRefs refs)
        {
            base.StartState(refs);

            if (refs.IsServer) SrvOnStartState();
        }

        public override void EndState()
        {
            if (playerRefs.IsServer) SrvOnEndState();

            base.EndState();
        }

        public override void Dispose()
        {
            base.Dispose();
            
            _deathTimer = null;
        }

        [Server]
        private void SrvOnStartState()
        {
            _deathTimer = new Timer();
            _deathTimer.StartTimerWithCallback(playerRefs.StateMachine, GameSettings.instance.deathTime, () =>
            {
                playerRefs.StateMachine.ChangeState(playerRefs.StateMachine.idleState);
            });

            _position = playerRefs.PlayerTransform.position;
            playerRefs.PlayerTransform.GetComponent<NetworkTransform>().Teleport(new Vector3(999, 999, 999), Quaternion.identity, Vector3.one);
        }

        [Server]
        private void SrvOnEndState()
        {
            playerRefs.PlayerTransform.GetComponent<NetworkTransform>().Teleport(_position, Quaternion.identity, Vector3.one);
            playerRefs.GetComponentInChildren<IHealable>().MaxHeal();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="newStateMachine"></param>
        /// <returns></returns>
        public override bool CanChangeStateTo(BaseStateMachine newStateMachine)
        {
            return false;
        }

        public override string ToString()
        {
            return "Dead";
        }
    }
}