using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーのボール所持・表示を管理するクラス
/// </summary>
public class PlayerBallHolder
{
    private readonly SO_PlayerSettings _settings;
    private readonly Transform _holdPointTransform;

    private int _ballCount;
    public int BallCount => _ballCount;
    public bool CanShoot => _ballCount > 0;

    private readonly List<GameObject> _holdBalls = new List<GameObject>();

    public PlayerBallHolder(SO_PlayerSettings settings, Transform holdPointTransform)
    {
        _settings = settings;
        _holdPointTransform = holdPointTransform;
    }

    /// <summary> ボール所持数を初期化する </summary>
    public void InitializeBalls(int count)
    {
        _ballCount = Mathf.Clamp(count, 0, _settings.MaxHoldCount);

        foreach (var ball in _holdBalls)
        {
            if (ball != null) Object.Destroy(ball);
        }
        _holdBalls.Clear();

        // 現在の所持数分だけ生成して holdPoint に配置
        for (int i = 0; i < _ballCount; i++)
        {
            CreateHoldBall(i);
        }
    }

    /// <summary> ボールを1つ消費する</summary>
    public bool TryConsumeBall()
    {
        if (_ballCount <= 0)　return false;
        _ballCount--;

        if (_holdBalls.Count > 0)
        {
            var lastBall = _holdBalls[_holdBalls.Count - 1];
            if (lastBall != null) Object.Destroy(lastBall);
            _holdBalls.RemoveAt(_holdBalls.Count - 1);
        }

        return true;
    }

    /// <summary> ボールを1つ追加する</summary>
    public bool TryAddBall()
    {
        if (_ballCount >= _settings.MaxHoldCount) return false;
        _ballCount++;

        CreateHoldBall(_ballCount - 1);
        return true;
    }

    private void CreateHoldBall(int index)
    {
        var holdBall = Object.Instantiate(_settings.BallPrefab, _holdPointTransform);
        holdBall.transform.localScale = Vector3.one * 0.2f;
        holdBall.transform.localPosition = new Vector3(index * 0.3f, 0, 0);

        // 所持状態のボールは物理挙動と当たり判定、自動消滅スクリプトを持たせないようにする
        if (holdBall.TryGetComponent<Rigidbody>(out var rb)) Object.Destroy(rb);
        if (holdBall.TryGetComponent<Collider>(out var col)) Object.Destroy(col);
        if (holdBall.TryGetComponent<BallController>(out var bc)) Object.Destroy(bc);

        _holdBalls.Add(holdBall);
    }
}
