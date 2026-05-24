using UnityEngine;

public class PlayerShootState : PlayerState
{
    public PlayerShootState(PlayerContext context) : base(context) { }
    
    public override void Enter()
    {
        if (!Context.BallHolder.TryConsumeBall())
        {
            // ボールがない場合は投げない
            Debug.LogWarning("[Player] ボールがありません");
            return;
        }

        Debug.Log($"<color=green>[Player] Enter: Shooting State</color> (残り: {Context.BallHolder.BallCount})");
        PerformThrow();
    }

    /// <summary>
    /// 投げる処理。
    /// プレイヤーの前方にボールを生成し、前方方向に力を加えて射出する。
    /// </summary>
    private void PerformThrow()
    {
        var settings = Context.Settings;

        // TODO : この手法での生成はパフォーマンスに影響が出る可能性があるため、
        //        必要に応じてオブジェクトプーリングの導入を検討すること
        var ball = Object.Instantiate(
            settings.BallPrefab, 
            Context.releasePointTransform.position,
            Quaternion.identity);

        // 投げた人自身との衝突を無視する（生成直後の誤判定防止）
        if (ball.TryGetComponent<Collider>(out var ballCollider))
        {
            Physics.IgnoreCollision(ballCollider, Context.CapsuleCollider);
        }

        if (ball.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(Camera.main.transform.forward * settings.ThrowForce, ForceMode.Impulse);
        }
    }

    public override void Execute()
    {
        // 投げ終わり後、移動入力があれば移動状態へ戻る（移動中に投げた場合の復帰）
        var moveInput = GetMoveInput();
        if (moveInput.magnitude >= Clash.Constants.PlayerConfig.INPUT_DEADZONE)
        {
            Context.TransitionTo<PlayerMoveState>();
            return;
        }

        Context.TransitionTo<PlayerIdleState>();
    }
}
