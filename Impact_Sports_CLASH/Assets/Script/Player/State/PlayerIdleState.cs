using UnityEngine;
using Clash.Constants;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerContext context) : base(context) { }

    public override void Enter()
    {
        Context.Rigidbody.linearVelocity = Vector3.zero;
    }

    public override void Execute()
    {
        var moveInput = GetMoveInput();

        // 射撃入力があり、ボールを所持していれば射撃状態へ遷移
        if (IsShotTriggered() && Context.BallHolder.CanShoot)
        {
            Context.TransitionTo<PlayerShootState>();
            return;
        }

        if (IsDodgeTriggered())
        {
            Context.TransitionTo<PlayerDodgeState>();
            return;
        }

        if (IsCatchTriggered())
        {
            Context.TransitionTo<PlayerCatchState>();
            return;
        }

        // 移動入力が一定以上あれば移動状態へ遷移
        if (moveInput.magnitude >= PlayerConfig.INPUT_DEADZONE)
        {
            Context.TransitionTo<PlayerMoveState>();
            return;
        }

    }
}
