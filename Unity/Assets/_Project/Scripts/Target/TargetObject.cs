using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class TargetObject : NetworkBehaviour, ITargetable
    {
        [SerializeField] private Outline outline;
        
        private Sequence _outlineSeq;
        
        private void Start()
        {
            outline.OutlineWidth = 0;
        }
        
        public void OnTargeted()
        {
            if (_outlineSeq != null && _outlineSeq.IsActive()) _outlineSeq.Kill();
            
            _outlineSeq = DOTween.Sequence();
            
            _outlineSeq.Append(DOTween.To(() => outline.OutlineWidth, (x) => outline.OutlineWidth = x,
                4, 0.15f));
        }

        public void OnUntargeted()
        {
            if (_outlineSeq != null && _outlineSeq.IsActive()) _outlineSeq.Kill();
            
            _outlineSeq = DOTween.Sequence();
            
            _outlineSeq.Append(DOTween.To(() => outline.OutlineWidth, (x) => outline.OutlineWidth = x, 0, 0.15f));
        }

        public NetworkObject GetNetworkObject() => NetworkObject;
    }
}