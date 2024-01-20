using Project;
using UnityEngine;

public class PlayerBaseState : PlayerState
{
    public override string ToString() { return "PlayerBaseState"; }
    protected PlayerBaseState(UserInstance newPlayer) : base(newPlayer){ }

    protected override void StartState(UserInstance player)
    {
        //base sate no hover state
        
        //assign player
        Player = player;
    }

    public override void UpdateState()
    {
        //Update of the state
    }

    public override void ChangeState(PlayerState newState)
    {
        Debug.Log("new state: " + newState);
    }

    public override bool CanChangeState(PlayerState newState)
    {
        return true;
    }
}
