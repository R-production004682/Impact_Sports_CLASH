using Clash.Constants;
using UnityEngine;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerContext context) : base(context){}

    public override void Execute()
    {
        var moveInput = GetMoveInput();
        if (Context.Rigidbody.linearVelocity.magnitude < PlayerConfig.STOP_VELOCITY_THRESHOLD)
        {
            Context.TransitionTo<PlayerIdleState>();
            return;
        }

        // 射撃入力があり、ボールを所持していれば射撃状態へ遷移（移動中でも投げられる）
        if (IsShotTriggered() && Context.BallHolder.CanShoot)
        {
            Context.TransitionTo<PlayerShootState>();
            return;
        }

        // NOTE : 必要に応じて下記のような処理を追加していく
        // ジャンプ入力があればジャンプ状態へ遷移
        // スライディング入力とかあれば、スライディング状態へ遷移
    }

    public override void FixedExecute()
    {
        var input = GetMoveInput();

        // TODO : 入力方向の変換は、カメラの向きに合わせる必要がある
        var inputDirection = new Vector3(input.x, 0, input.y).normalized;
        
        var rb = Context.Rigidbody;
        var currentVelocity = rb.linearVelocity;

        var currentHorizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        // 摩擦による減速
        if (inputDirection.magnitude == 0)
        {
            currentHorizontalVelocity = Vector3.Lerp(
                currentHorizontalVelocity, 
                Vector3.zero, 
                Context.Settings.GrandFriction * Time.fixedDeltaTime);
        }

        var targetVelocity 
            = currentHorizontalVelocity + inputDirection * (Context.Settings.Acceleration * Time.fixedDeltaTime);

        // 最大速度のクランプ
        if (targetVelocity.magnitude > Context.Settings.MaxMoveSpeed)
        {
            targetVelocity = targetVelocity.normalized * Context.Settings.MaxMoveSpeed;
        }

        rb.linearVelocity = new Vector3(targetVelocity.x, currentVelocity.y, targetVelocity.z);
    }
}
