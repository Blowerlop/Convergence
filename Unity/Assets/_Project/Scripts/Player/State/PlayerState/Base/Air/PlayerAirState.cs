using Project;
using UnityEngine;

public class PlayerAirState : PlayerBaseState //TODO: maybe with buff????
{
    public override string ToString() { return "PlayerAirState"; }
    
    public PlayerAirState(UserInstance newPlayer) : base(newPlayer) { }

    protected override void StartState(UserInstance newPlayer)
    {
        base.StartState(newPlayer);
        Debug.Log("air");
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
        return newState is PlayerAirState && base.CanChangeState(newState);
    }
}
