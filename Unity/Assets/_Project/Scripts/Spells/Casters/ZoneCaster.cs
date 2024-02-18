using UnityEngine;

namespace Project.Spells.Casters
{
    public class ZoneCaster : SpellCaster
    {
        [SerializeField] private Transform aimVisual;
        [SerializeField] private Transform zoneVisual;
        
        [SerializeField] private LayerMask groundLayerMask;
        private Camera _camera;
        
        private SingleVectorResults _currentResults = new();

        ZoneSpellData _zoneSpellData;
        
        private void Start()
        {
            _camera = Camera.main;
            zoneVisual.gameObject.SetActive(false);
            aimVisual.gameObject.SetActive(false);
        }

        public override void Init(Transform casterTransform, SpellData spell)
        {
            base.Init(casterTransform, spell);
            
            var zoneSpell = spell as ZoneSpellData;

            if (zoneSpell == null)
            {
                Debug.LogError("DefaultZoneCaster can only be used with DefaultZoneData. You gave spell: " +
                               spell.name + " of Type: " + spell.GetType());
                return;
            }

            _zoneSpellData = zoneSpell;
            
            zoneVisual.localScale = Vector3.one * zoneSpell.limitRadius * 2;
            aimVisual.localScale = Vector3.one * zoneSpell.zoneRadius * 2;
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
        
        protected override void UpdateChanneling()
        {
            var pos = _currentResults.VectorProp;
            pos.y = aimVisual.position.y;
            
            aimVisual.position = pos;
        }

        public override void EvaluateResults()
        {
            Utilities.GetMouseWorldPosition(_camera, groundLayerMask, out Vector3 position);
            
            var zoneCenter = zoneVisual.position;
            position = zoneCenter + Vector3.ClampMagnitude(position - zoneCenter, _zoneSpellData.limitRadius);
            
            _currentResults.VectorProp = position;
        }

        public override void TryCast(int casterIndex)
        {
            SpellManager.instance.TryCastSpellServerRpc(casterIndex, _currentResults);
        }
    }
}