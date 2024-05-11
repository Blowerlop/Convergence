using Project._Project.Scripts.StateMachine;

namespace Project._Project.Scripts.Player.States
{
    public class ChannelingState : BaseStateMachineBehaviour
    {
        private readonly byte _index;
        
        
        public ChannelingState(byte index)
        {
            _index = index;
        }
        
        
        protected override void OnEnter()
        {
            playerRefs.Animator.SetBool("Channeling " + _index, true);
        }
        
        protected override void OnExit()
        {
            playerRefs.Animator.SetBool("Channeling " + _index, false);
        }
        
        public override bool CanChangeStateTo<T>()
        {
            return typeof(T) == typeof(IdleState);
        }

        public override string ToString()
        {
            return "Casting";
        }

        public override bool Equals<T>(T obj)
        {
            return base.Equals(obj) && _index == (obj as ChannelingState)._index;
        }
    }
}
