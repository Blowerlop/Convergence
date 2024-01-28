using Project;
using UnityEngine;

public class PlayerCastingState : PlayerBaseState //TODO: maybe useless
{
    public override string ToString() { return "PlayerCastingState"; }

    public override void StartState(MovementController newPlayer)
    {
        base.StartState(newPlayer);
        Debug.Log("Casting");
        //Player.character.anim.SetTrigger("TODO");
    }
    public override void UpdateState()
    {
        base.UpdateState();
    }

    public override bool CanChangeState(PlayerState newState)
    {
        return newState is not PlayerState && base.CanChangeState(newState); //TODO: good state
    }

    public override void ChangeState(PlayerState newState)
    {
        base.ChangeState(newState);
    }
}
