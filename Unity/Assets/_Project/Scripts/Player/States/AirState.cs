using Project._Project.Scripts.StateMachine;
using UnityEngine;

namespace Project._Project.Scripts.Player.State.PlayerState.Base.Air
{
    public class AirState : BaseStateMachine //TODO: maybe with buff????
    {
        public override bool CanChangeStateTo(BaseStateMachine newStateMachine)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return "Air";
        }
    }
}