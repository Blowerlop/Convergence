using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    public class TargetingController : MonoSingleton<TargetingController>
    {
        [SerializeField] private Texture2D defaultTargetCursor;
        [SerializeField] private Texture2D defaultCustomNoTargetCursor;
        [SerializeField] private Texture2D defaultCustomTargetCursor;

        [SerializeField] private LayerMask targetableLayerMask;
        
        private Predicate<ITargetable> _currentPredicate;
        private ITargetable _currentResult;

        private Texture2D _currentNoTargetCursor, _currentTargetCursor;

        private const string TargetCursorId = "TargetCursorId";
        private const string NoTargetCursorId = "NoTargetCursorId";
        
        private void Start()
        {
            ResetCursors();
        }   
        
        /// <summary>
        /// Start targeting process. Target are found based on predicate.
        /// If predicate is null, all ITargetable are valid.
        /// </summary>
        [Button]
        public void BeginCustom(Predicate<ITargetable> targetingPredicate = null)
        {
            BeginCustom(defaultCustomNoTargetCursor, defaultCustomTargetCursor, targetingPredicate);
        }

        public void BeginCustom(Texture2D customNoTargetCursor, Texture2D customTargetCursor, 
            Predicate<ITargetable> targetingPredicate = null)
        {
            _currentNoTargetCursor = customNoTargetCursor;
            _currentTargetCursor = customTargetCursor;
            
            CursorManager.Request(NoTargetCursorId, _currentNoTargetCursor, Vector2.one * 32, 
                CursorMode.Auto, CursorLockMode.Confined);
            
            _currentPredicate = targetingPredicate;
        }

        private void Update()
        { 
            var lastTarget = _currentResult;
            bool hadTarget = lastTarget != null;
            
            if (TryGetResult(out _currentResult))
            {
                if (!hadTarget)
                {
                    _currentResult.OnTargeted();
                    CursorManager.Request(TargetCursorId, _currentTargetCursor, Vector2.one * 32, 
                        CursorMode.Auto, CursorLockMode.Confined);
                }
            }
            else if (hadTarget)
            { 
                lastTarget.OnUntargeted();
                CursorManager.Release(TargetCursorId);
            }
        }
        
        /// <summary>
        /// Get TargetingResult based on where the cursor is pointing at.
        /// You can get TargetingResult without ending the targeting process.
        /// </summary>
        /// <returns></returns>
        public bool TryGetResult(out ITargetable result)
        {
            if (Utilities.GetFirstHitFromMouse(Camera.main, targetableLayerMask, out var hit))
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
        public void StopCustom()
        {
            CursorManager.Release(NoTargetCursorId);
            CursorManager.Release(TargetCursorId);
            _currentPredicate = null;

            ResetCursors();
        }

        private void ResetCursors()
        {
            _currentTargetCursor = defaultTargetCursor;
            _currentNoTargetCursor = null;
        }
    }
}