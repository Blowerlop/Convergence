using Project;

public abstract class PlayerState
{
    /// <summary>
    /// State owner
    /// </summary>
    protected MovementController Player; //TODO maybe get the player by another way but actually we get it with the param

    /// <summary>
    /// Called on start state (not the same as the Unity event "Start")
    /// </summary>
    /// <param name="player">State owner => <see cref="Player"/></param>
    public abstract void StartState(MovementController player);
    
    public override string ToString() { return "PlayerState"; } //DEBUG To REMOVE

    /// <summary>
    /// Called with the Unity event "Update"
    /// </summary>
    public abstract void UpdateState();
    
    /// <summary>
    /// Called on state end
    /// </summary>
    protected abstract void EndState();
    
    /// <summary>
    /// <para>Switch this state with another: <paramref name="newState"/> </para>
    /// /!\ do a <see cref="CanChangeState"/> before
    /// </summary>
    /// <param name="newState">new player state</param>
    public abstract void ChangeState(PlayerState newState);
    
    /// <summary>
    /// Check if we can change the actual state with the <paramref name="newState"/>
    /// </summary>
    /// <param name="newState">new player state to compare</param>
    /// <returns><para><b>-true</b>: if the change is allowed</para>
    /// <para><b>-false</b>: if the change is not allowed</para></returns>
    public abstract bool CanChangeState(PlayerState newState);
}
