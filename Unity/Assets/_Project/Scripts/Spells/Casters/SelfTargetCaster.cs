using System;

namespace Project.Spells.Casters
{
    public class SelfTargetCaster : SpellCaster
    {
        public override Type CastResultType => typeof(EmptyResults);
        
        private EmptyResults _currentResults = new();
        
        protected override void UpdateChanneling() { }

        public override void EvaluateResults() { }

        public override void TryCast(int casterIndex)
        {
            SpellManager.instance.TryCastSpellServerRpc(casterIndex, _currentResults);
        }
    }
}