namespace Project.Spells.Casters
{
    public class SelfTargetCaster : SpellCaster
    {
        private EmptyResults _currentResults = new();
        
        protected override void UpdateChanneling() { }

        public override void EvaluateResults() { }

        public override void TryCast(int casterIndex)
        {
            SpellManager.instance.TryCastSpellServerRpc(casterIndex, _currentResults);
        }
    }
}