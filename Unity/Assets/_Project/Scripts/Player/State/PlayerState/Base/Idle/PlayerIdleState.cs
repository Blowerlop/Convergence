using System.Collections;
using System.Collections.Generic;
using Project;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public override string ToString() { return "PlayerIdleState"; }
    
    public PlayerIdleState(UserInstance newPlayer) : base(newPlayer) { }

    protected override void StartState(UserInstance newPlayer)
    {
        base.StartState(newPlayer);
        Debug.Log("idle");
    }
    public override void UpdateState()
    {
        base.UpdateState();
    }

    public override void ChangeState(PlayerState newState)
    {
        base.ChangeState(newState);
    }
}
