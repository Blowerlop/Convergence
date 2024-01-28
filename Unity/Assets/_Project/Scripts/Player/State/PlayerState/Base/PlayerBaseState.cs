using Project;
using UnityEngine;

public class PlayerBaseState : PlayerState
{
    public override string ToString() { return "PlayerBaseState"; }

    public override void StartState(MovementController player)
    {
        //base sate no hover state
        
        //assign player
        Player = player;
    }

    public override void UpdateState()
    {
        //Update of the state
    }

    protected override void EndState()
    {
        Debug.Log("end " + this);
    }

    public override void ChangeState(PlayerState newState)
    {
        Debug.Log("new state: " + newState);
        EndState();
        Player.state = newState;
        newState.StartState(Player);
    }

    public override bool CanChangeState(PlayerState newState)
    {
        return true;
    }
}
