public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerContext context) : base(context) { }

    public override void Execute()
    {
        if (IsShotTriggered())
        {
            Context.TransitionTo<PlayerShootState>();
        }
    }
}
