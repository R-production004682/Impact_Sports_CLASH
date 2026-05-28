using Clash.Constants;
using UnityEngine;
public class PlayerDodgeState : PlayerState
{
    private float _timer;
    private Vector3 _dodgeDirection;

    public PlayerDodgeState(PlayerContext context) : base(context){}

    public override void Enter()
    {
        _timer = 0f;
        _dodgeDirection = ResolveDodgeDirection();

        ApplyDodgeVelocity();
    }
    
    public override void Execute()
    {
        _timer += Time.deltaTime;
        if (_timer >= Context.Settings.DodgeDuration)
        {
            if (GetMoveInput().magnitude >= PlayerConfig.INPUT_DEADZONE)
            {
                Context.TransitionTo<PlayerMoveState>();
            }
            else
            {
                Context.TransitionTo<PlayerIdleState>();
            }
        }
    }

    public override void FixedExecute() => ApplyDodgeVelocity();

    /// <summary>
    /// 回避時の速度を適用する
    /// </summary>
    private void ApplyDodgeVelocity()
    {
        var velocity = Context.Rigidbody.linearVelocity;

        Context.Rigidbody.linearVelocity =
            new Vector3(
                _dodgeDirection.x * Context.Settings.DodgeSpeed,
                velocity.y,
                _dodgeDirection.z * Context.Settings.DodgeSpeed
            );
    }


    /// <summary>
    /// 回避方向を決定させる
    /// </summary>
    /// <returns></returns>
    private Vector3 ResolveDodgeDirection()
    {
        var input = GetMoveInput();

        // 入力方向が無入力だった場合は前方方向に回避
        if (input.magnitude < PlayerConfig.INPUT_DEADZONE)
        {
            input = Vector2.up;
        }

        // TODO : 移動方向をカメラを正にする対応時に、ここもついでにカメラ方向に回避するようにする
        var forward = Context.Transform.forward;
        forward.y = 0;
        forward.Normalize();

        var right = Context.Transform.right;
        right.y = 0;
        right.Normalize();

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            return input.x > 0 ? right : -right;
        }

        return input.y > 0 ? forward : -forward;
    }
}
