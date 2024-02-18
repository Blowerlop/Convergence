using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    public class TargetingController : MonoSingleton<TargetingController>
    {
        [SerializeField] private Texture2D _noTargetCursor, hasTargetCursor;
        
        [SerializeField] private LayerMask _targetableLayerMask;
        
        private bool _isTargeting;
        
        private Func<ITargetable, bool> _currentPredicate;
        private ITargetable _currentResult;
        
        /// <summary>
        /// Start targeting process. Target are found based on predicate.
        /// If predicate is null, all ITargetable are valid.
        /// </summary>
        [Button]
        public void Begin(Func<ITargetable, bool> targetingPredicate = null)
        {
            if (_isTargeting) return;
            
            CursorManager.Request(_noTargetCursor, Vector2.one * 32, CursorMode.Auto, CursorLockMode.Confined);
            
            _currentPredicate = targetingPredicate;
            _isTargeting = true;
        }

        private void Update()
        {
            if (!_isTargeting) return;

            var lastTarget = _currentResult;
            bool hadTarget = lastTarget != null;
            
            if (TryGetResult(out _currentResult))
            {
                if (!hadTarget)
                {
                    _currentResult.OnTargeted();
                    CursorManager.Request(hasTargetCursor, Vector2.one * 32, CursorMode.Auto, CursorLockMode.Confined);
                }
            }
            else if (hadTarget)
            { 
                lastTarget.OnUntargeted();
                CursorManager.Release();
            }
        }
        
        /// <summary>
        /// Get TargetingResult based on where the cursor is pointing at.
        /// You can get TargetingResult without ending the targeting process.
        /// </summary>
        /// <returns></returns>
        public bool TryGetResult(out ITargetable result)
        {
            if (Utilities.GetFirstHitFromMouse(Camera.main, _targetableLayerMask, out var hit))
            {
                if (hit.transform.TryGetComponent(out result))
                {
                    return _currentPredicate == null || _currentPredicate.Invoke(_currentResult);
                }
            }
            
            result = null;
            return false;
        }

        /// <summary>
        /// Called to stop the targeting process
        /// </summary>
        [Button]
        public void Stop()
        {
            if (!_isTargeting) return;
            
            _isTargeting = false;
            CursorManager.Release();
        }
    }
}