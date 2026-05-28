using UnityEngine;
using Clash.Constants;

public class PlayerCatchState : PlayerState
{
    private float _timer;

    // OverlapSphereNonAlloc 用の再利用バッファ
    private static readonly Collider[] _overlapBuffer = new Collider[20];

    public PlayerCatchState(PlayerContext context) : base(context) { }

    public override void Enter()
    {
        _timer = 0f;

        // キャッチ構え：速度を止める
        Context.Rigidbody.linearVelocity = Vector3.zero;
    }

    public override void Execute()
    {
        _timer += Time.deltaTime;

        if (_timer >= Context.Settings.CatchWindowDuration)
        {
            Context.TransitionTo<PlayerIdleState>();
        }
    }

    public override void FixedExecute()
    {
        TryCatchBall();
    }

    /// <summary>
    /// プレイヤー前方に OverlapSphere を実行し、"Ball" タグの物体を探す。
    /// 見つかれば捕球
    /// </summary>
    private void TryCatchBall()
    {
        var settings = Context.Settings;
        var center = Context.Transform.position + Context.Transform.forward * settings.CatchForwardOffset;

        var hitCount = Physics.OverlapSphereNonAlloc(center, settings.CatchRadius, _overlapBuffer);

        for (var i = 0; i < hitCount; i++)
        {
            var col = _overlapBuffer[i];

            if (!col.CompareTag("Ball"))
            {
                continue;
            }

            if (!Context.BallHolder.TryAddBall())
            {
                continue;
            }

            Debug.Log($"<color=cyan>[Player] ボールをキャッチ！ (所持: {Context.BallHolder.BallCount})</color>");
            // TOTO : オブジェクトプール
            Object.Destroy(col.gameObject);

            Context.TransitionTo<PlayerIdleState>();
            return;
        }
    }
}
