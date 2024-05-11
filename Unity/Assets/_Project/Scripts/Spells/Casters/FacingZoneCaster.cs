using System;
using UnityEngine;

namespace Project.Spells.Casters
{
    public class FacingZoneCaster : SpellCaster
    {
        public override Type CastResultType => typeof(SingleVectorResults);
        
        [SerializeField] private Transform aimVisual;
        [SerializeField] private Transform zoneVisual;
        
        [SerializeField] private LayerMask groundLayerMask;
        private Camera _camera;
        
        private SingleVectorResults _currentResults = new();

        FacingZoneSpellData _spellData;
        
        private void Start()
        {
            _camera = Camera.main;
            zoneVisual.gameObject.SetActive(false);
            aimVisual.gameObject.SetActive(false);
        }

        public override void Init(Transform casterTransform, SpellData spell)
        {
            base.Init(casterTransform, spell);
            
            var zoneSpell = spell as FacingZoneSpellData;

            if (zoneSpell == null)
            {
                Debug.LogError("DefaultZoneCaster can only be used with DefaultZoneData. You gave spell: " +
                               spell.name + " of Type: " + spell.GetType());
                return;
            }

            _spellData = zoneSpell;
            
            zoneVisual.localScale = Vector3.one * zoneSpell.limitRadius * 2;
            aimVisual.localScale = Vector3.one * zoneSpell.zoneSize;
        }
        
        public override void StartCasting()
        {
            if (IsCasting) return;
            
            base.StartCasting();
            zoneVisual.gameObject.SetActive(true);
            aimVisual.gameObject.SetActive(true);
        }
        
        public override void StopCasting()
        {
            if (!IsCasting) return;
            
            base.StopCasting();
            zoneVisual.gameObject.SetActive(false);
            aimVisual.gameObject.SetActive(false);
        }
        
        protected override void UpdateCasting()
        {
            var pos = _currentResults.VectorProp;
            pos.y = aimVisual.position.y;

            var dir = pos - zoneVisual.position;
            dir.y = 0;
            dir.Normalize();
            
            aimVisual.position = pos;
            aimVisual.rotation = Quaternion.LookRotation(dir);
        }

        public override void EvaluateResults()
        {
            Utilities.GetMouseWorldPosition(_camera, groundLayerMask, out Vector3 position);
            
            var zoneCenter = zoneVisual.position;
            position = zoneCenter + Vector3.ClampMagnitude(position - zoneCenter, _spellData.limitRadius);
            
            _currentResults.VectorProp = position;
        }

        public override void TryCast(int casterIndex)
        {
            SpellManager.instance.TryCastSpellServerRpc(casterIndex, _currentResults);
        }
    }
}