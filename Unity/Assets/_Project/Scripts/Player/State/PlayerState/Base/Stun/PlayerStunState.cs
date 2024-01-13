using Project;
using UnityEngine;

/*public class PlayerStunState : PlayerBaseState TODO: useless (done with buff)
{
    public override string ToString() { return "PlayerStunState"; }
    
    public PlayerStunState(PlayerState newOverState, UserInstance newPlayer) : base(newOverState, newPlayer) { }

    protected override void StartState(PlayerState newOverState, UserInstance newPlayer)
    {
        base.StartState(newOverState, newPlayer);
        Debug.Log("stun");
        Player.character.anim.SetBool("IsStunned", true);
    }
    public override void UpdateState()
    {
        SubState?.UpdateState();
    }

    public override void ChangeState(PlayerState newState)
    {
        base.ChangeState(newState);
        Player.character.anim.SetBool("IsStunned", false);
    }
}*/
