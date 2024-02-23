using UnityEngine;

namespace Project.Spells.Casters
{
    public class TargetCaster : SpellCaster
    {
        // We don't need to get target every frame. Just when we cast the spell
        protected override bool DisableUpdateEvaluation => true;
        
        [SerializeField] private Transform zoneVisual;
        
        private SingleUIntResults _currentResults = new();

        TargetSpellData _targetSpellData;
        
        private void Start()
        {
            zoneVisual.gameObject.SetActive(false);
        }

        public override void Init(Transform casterTransform, SpellData spell)
        {
            base.Init(casterTransform, spell);
            
            var targetSpell = spell as TargetSpellData;

            if (targetSpell == null)
            {
                Debug.LogError("TargetCaster can only be used with TargetSpellData. You gave spell: " +
                               spell.name + " of Type: " + spell.GetType());
                return;
            }

            _targetSpellData = targetSpell;
            
            zoneVisual.localScale = Vector3.one * _targetSpellData.zoneRadius * 2;
        }

        public override void StartCasting()
        {
            if (IsCasting) return;

            base.StartCasting();
            zoneVisual.gameObject.SetActive(true);

            TargetingController.instance.BeginCustom();
        }

        public override bool StopCasting()
        {
            if(!TargetingController.instance.TryGetResult(out var _)) 
                return false;
            
            if (!base.StopCasting()) return false;
            
            zoneVisual.gameObject.SetActive(false);
            
            TargetingController.instance.StopCustom();

            return true;
        }
        
        protected override void UpdateChanneling() { }

        public override void EvaluateResults()
        {
            TargetingController.instance.TryGetResult(out var targetingResult);
            
            _currentResults.UIntProp = (uint)targetingResult.GetNetworkObject().NetworkObjectId;
        }

        public override void TryCast(int casterIndex)
        {
            SpellManager.instance.TryCastSpellServerRpc(casterIndex, _currentResults);
        }
    }
}