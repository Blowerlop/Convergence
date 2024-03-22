using DG.Tweening;
using UnityEngine;

namespace Project._Project.Scripts
{
    public class Dummy : Entity
    {
        [SerializeField] private SOEntity entityData;
        
        private Sequence _shakeSeq;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                ServerInit(entityData);
            }
            
            _stats.Get<HealthStat>().OnValueChanged += OnHealthChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _stats.Get<HealthStat>().OnValueChanged -= OnHealthChanged;
        }
        
        private void OnHealthChanged(int oldValue, int newValue)
        {
            if(_shakeSeq != null && _shakeSeq.IsActive()) _shakeSeq.Kill(complete: true);
            
            _shakeSeq = DOTween.Sequence();
            _shakeSeq.Join(transform.DOShakeRotation(0.35f, Vector3.one * 5f, 15, 90f, true));
        }
    }
}