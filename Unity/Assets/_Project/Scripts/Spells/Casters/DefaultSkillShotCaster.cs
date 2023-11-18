using Project._Project.Scripts.Spells.Results;
using UnityEngine;

namespace Project._Project.Scripts.Spells.Casters
{
    public class DefaultSkillShotCaster : SpellCaster
    {
        [SerializeField] private Transform visual;
        
        [SerializeField] private LayerMask groundLayerMask;
        private Camera _camera;
        
        private DefaultSkillShotResults _currentResults = new();

        private void Start()
        {
            _camera = Camera.main;
            visual.gameObject.SetActive(false);
        }
        
        public override void StartChanneling()
        {
            if (IsChanneling) return;
            
            base.StartChanneling();
            visual.gameObject.SetActive(true);
        }
        
        public override void StopChanneling()
        {
            if (!IsChanneling) return;
            
            base.StopChanneling();
            visual.gameObject.SetActive(false);
        }
        
        protected override void UpdateChanneling()
        {
            visual.rotation = Quaternion.LookRotation(_currentResults.Direction);
        }

        protected override void EvaluateResults()
        {
            Utilities.GetMouseWorldPosition(_camera, groundLayerMask, out Vector3 position);
            position.y = 0;

            var playerPos = CasterTransform.position;
            playerPos.y = 0;
            
            _currentResults.Direction = (position - playerPos).normalized;
        }

        public override ICastResult GetResults()
        {
            EvaluateResults();
            return _currentResults;
        }
    }
}