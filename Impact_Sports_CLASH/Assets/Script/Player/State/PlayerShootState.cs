using UnityEngine;

public class PlayerShootState : PlayerState
{
    public PlayerShootState(PlayerContext context) : base(context) { }
    
    public override void Enter()
    {
        Debug.Log("<color=green>[Player] Enter: Shooting State</color>");
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
            Context.Transform.position + Context.Transform.up,  // 射出位置を一旦プレイヤーの頭上に設定
            Quaternion.identity);

        if (ball.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(Context.Transform.forward * settings.ThrowForce, ForceMode.Impulse);
        }
    }

    public override void Execute()
    {
        // 投げ終わりのモーション時間待機などの後、Idleへ戻る（今回は即時）
        Context.TransitionTo<PlayerIdleState>();
    }
}
