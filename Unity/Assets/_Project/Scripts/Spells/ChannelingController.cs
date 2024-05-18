using System;
using Project._Project.Scripts.Player.States;
using Project._Project.Scripts.StateMachine;
using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public class ChannelingController : NetworkBehaviour
    { 
        [SerializeField] private PlayerRefs playerRefs;
        
        [SerializeField] private string channelingNetVarName = "Channeling";
        
        private GRPC_NetworkVariable<bool> _isChanneling;
        
        private readonly Timer _channelingTimer = new Timer();
        
        public event Action OnServerChannelingStarted; 
        public event Action OnServerChannelingEnded;

        public bool IsChanneling => _isChanneling.Value;
        
        private void Awake()
        {
            _isChanneling = new GRPC_NetworkVariable<bool>(channelingNetVarName);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            InitNetVars();
            
            if (!IsClient) return;

            _isChanneling.OnValueChanged += (_, newValue) =>
            {
                 if (newValue) OnServerChannelingStarted?.Invoke();
                 else OnServerChannelingEnded?.Invoke();
            };
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            _isChanneling.Reset();
        }
        
        private void InitNetVars()
        {
            _isChanneling.Initialize();
        }
        
        [Server]
        public void StartServerChanneling(float channelingTime, byte index, Action channelingDoneAction = null)
        {
            // Bypass entering channeling state and exiting it instantly if we have no channeling time
            if (channelingTime == 0)
            {
                GoToIdle();
                
                channelingDoneAction?.Invoke();
                return;
            }
            
            ChangeState(new ChannelingState((byte)(index + 1)));
            
            _isChanneling.Value = true;
            _channelingTimer.StartTimerWithCallback(this, channelingTime, () =>
            {
                GoToIdle();
                
                channelingDoneAction?.Invoke();
                _isChanneling.Value = false;
            });

            void ChangeState(ChannelingState channelingState)
            {
                if (playerRefs is not PCPlayerRefs pcRefs) return;
                
                if (pcRefs.StateMachine.CanChangeStateTo<ChannelingState>())
                {
                    pcRefs.StateMachine.ChangeStateTo(channelingState);
                }
            }

            void GoToIdle()
            {
                if (playerRefs is not PCPlayerRefs pcRefs) return;
                
                if (pcRefs.StateMachine.CanChangeStateTo<IdleState>())
                {
                    pcRefs.StateMachine.ChangeStateTo<IdleState>();
                }
            }
        }
    }
}