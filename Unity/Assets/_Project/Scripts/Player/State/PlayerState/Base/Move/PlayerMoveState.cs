using Project;
using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    public override string ToString() { return "PlayerStunState"; }

    public override void StartState(MovementController player)
    {
        base.StartState(player);
        Debug.Log("Move");
    }

    public override void UpdateState()
    {
        base.UpdateState();
    }

    public override void ChangeState(PlayerState newState)
    {
        base.ChangeState(newState);
    }

    public override bool CanChangeState(PlayerState newState)
    {
        return newState is not PlayerAirState && base.CanChangeState(newState);
    }
}
