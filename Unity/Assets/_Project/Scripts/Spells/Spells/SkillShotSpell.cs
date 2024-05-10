using DG.Tweening;
using Project._Project.Scripts;
using Project._Project.Scripts.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Spells
{
    public class SkillShotSpell : Spell
    {
        private GRPC_NetworkVariable<bool> _impact = new GRPC_NetworkVariable<bool>("Impact");
        
        [Title("Moving Phase")]
        
        [SerializeField] private GameObject _movingObject;
        
        [SerializeField] private Vector3 _castOffset;
        [SerializeField] private float _castRadius;
        
        [SerializeField] private float speed = 3f;
        [SerializeField] private float moveDuration = 2f;

        [Title("Impact Phase")] 
        
        [SerializeField] private bool hasImpactPhase;
        
        [SerializeField, ShowIf(nameof(hasImpactPhase))] private GameObject _impactObject;
        [SerializeField, ShowIf(nameof(hasImpactPhase))] private float _impactDuration = 1f;

        private Transform _impactHitTransform;
        private bool _isOnImpactPhase;
        
        private SingleVectorResults _results;
        private Sequence _moveSeq;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            InitAudio();
            
            _movingObject.SetActive(true);
            if (hasImpactPhase) _impactObject.SetActive(false);
            
            _impact.Initialize();
            _impact.OnValueChanged += OnImpactChanged;
        }

        protected override void Init(ICastResult castResult)
        {
            if (castResult is not SingleVectorResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(SkillShotSpell)}!");
                return;
            }

            _results = results;

            _moveSeq = DOTween.Sequence();
            
            var dir = GetDirection(castResult, Caster);
            
            _moveSeq.Join(transform.DOMove(transform.position + dir * speed, moveDuration).SetEase(Ease.Linear));
            _moveSeq.OnComplete(KillSpell);
        }

        public override (Vector3, Quaternion) GetDefaultTransform(ICastResult castResult, PlayerRefs player)
        {
            if (castResult is not SingleVectorResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(SkillShotSpell)}!");
                return default;
            }
            
            return (player.PlayerTransform.position, Quaternion.LookRotation(GetDirection(castResult, player)));
        }

        public override Vector3 GetDirection(ICastResult castResult, PlayerRefs player)
        {
            if (castResult is not SingleVectorResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(SkillShotSpell)}!");
                return default;
            }
            
            var dir = results.VectorProp - player.PlayerTransform.position;
            dir.y = 0;
            dir.Normalize();
            
            return dir;
        }

        private void Update()
        {
            if (!IsServer && !IsHost) return;

            if (_isOnImpactPhase)
            {
                transform.position = _impactHitTransform.position;
                
                return;
            }
            
            if (IsColliding(out var hit)) SrvOnCollision(hit);
        }
        
        private bool IsColliding(out RaycastHit hit)
        {
            var forward = transform.forward;
            Vector3 realCastOffset = new Vector3(forward.x * _castOffset.x, 0, forward.z * _castOffset.z);
            
            return Physics.SphereCast(transform.position + realCastOffset, _castRadius, _results.VectorProp, 
                    out hit, 0.5f, Constants.Layers.EntityMask);
        }

        [Server]
        private void SrvOnCollision(RaycastHit hit)
        {
            if (!hit.transform.TryGetComponent(out Entity entity)) return;

            if (TryApplyEffects(entity))
            {
                if (!SrvCheckForImpactPhase(entity.transform))
                    KillSpell();
            }
        }

        [Server]
        private bool SrvCheckForImpactPhase(Transform target)
        {
            if (!hasImpactPhase) return false;
            
            // Set impact phase on all observers
            _impact.Value = true;
            
            _isOnImpactPhase = true;
            _impactHitTransform = target;
            
            _moveSeq.Kill();
            
            StartCoroutine(Utilities.WaitForSecondsAndDoActionCoroutine(_impactDuration, KillSpell));
            return true;
        }

        private void OnImpactChanged(bool oldValue, bool newValue)
        {
            if (!newValue) return;
            
            if(!hasImpactPhase) return;
            
            _movingObject.SetActive(false);
            _impactObject.SetActive(true);
        }
        
        private void KillSpell()
        {
            _moveSeq.Kill();
            NetworkObject.Despawn();
        }
        
        #region Audio

        private void InitAudio()
        {
            SoundManager.instance.PlayStaticSound(Data.spellId, gameObject, SoundManager.EventType.SFX);
        }
        
        #endregion
    }
}
