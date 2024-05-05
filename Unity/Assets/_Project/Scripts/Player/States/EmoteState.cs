using Project._Project.Scripts.StateMachine;
using UnityEngine;

namespace Project._Project.Scripts.Player.States
{
    public class EmoteState : BaseStateMachineBehaviour
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

        public override bool CanChangeStateTo<T>()
        {
            return true;
        }

        public override string ToString()
        {
            return "Emote State";
        }

        public override bool Equals<T>(T obj)
        {
            // ReSharper disable once PossibleNullReferenceException
            // Pas possible d'avoir une null ref on check le type dans le base 
            return base.Equals(obj) && emoteIndex == (obj as EmoteState).emoteIndex;
        }
    }
}