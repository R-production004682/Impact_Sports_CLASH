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

        // 移動入力が一定以上あれば移動状態へ遷移
        if (moveInput.magnitude >= PlayerConfig.MOVE_THRESHOLD)
        {
            Context.TransitionTo<PlayerMoveState>();
            return;
        }

        // 射撃入力があれば射撃状態へ遷移
        if (IsShotTriggered())
        {
            Context.TransitionTo<PlayerShootState>();
            return;
        }
    }
}
