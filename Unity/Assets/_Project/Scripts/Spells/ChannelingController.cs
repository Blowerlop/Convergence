using System;
using Unity.Netcode;
using UnityEngine;

namespace Project._Project.Scripts.Spells
{
    public class ChannelingController : NetworkBehaviour
    { 
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
        
        private void InitNetVars()
        {
            _isChanneling.Initialize();
        }
        
        public void StartServerChanneling(float channelingTime, Action channelingDoneAction = null)
        {
            if (!IsHost && !IsServer) return;
            
            _isChanneling.Value = true;
            _channelingTimer.StartTimerWithCallback(this, channelingTime, () =>
            {
                channelingDoneAction?.Invoke();
                _isChanneling.Value = false;
            });
        }
    }
}