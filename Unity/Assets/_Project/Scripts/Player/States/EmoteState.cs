using Project._Project.Scripts.StateMachine;

namespace Project._Project.Scripts.Player.States
{
    public class EmoteState : BaseStateMachine
    {
        private int emoteIndex;

        public EmoteState(int emoteIndex)
        {
            this.emoteIndex = emoteIndex;
        }

        protected override void OnEnter()
        {
            playerRefs.Animator.SetInteger(Constants.AnimatorsParam.EmoteIndex, emoteIndex);
        }

        protected override void OnExit()
        {
            playerRefs.Animator.SetInteger(Constants.AnimatorsParam.EmoteIndex, -1);
        }

        public override bool CanChangeStateTo(BaseStateMachine newStateMachine)
        {
            return true;
        }

        public override string ToString()
        {
            return "Emote State";
        }
    }
}