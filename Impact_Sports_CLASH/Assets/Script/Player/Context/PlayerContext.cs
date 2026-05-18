using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IPlayerContext
{
    PlayerInput PlayerInput { get; }
    CapsuleCollider CapsuleCollider { get; }
    Transform Transform { get; }
    SO_PlayerSettings Settings { get; }
    PlayerState CurrentState { get; }
    void TransitionTo<T>() where T : PlayerState;

    // 状態インスタンスのカスタム生成を登録可能にする（テストや最適化で利用予定）
    void RegisterStateFactory<T>(Func<IPlayerContext, PlayerState> factory) where T : PlayerState;
}

/// <summary>
/// Playerの状態やコンポーネントへのアクセスをまとめるコンテナで、
/// 必要最小限の公開 API を提供し、Context 依存の肥大化を抑える。
/// 
/// 状態生成はファクトリ登録による差し替えが可能で、将来状態数が増えても管理しやすくできるように設計
/// </summary>
public class PlayerContext : IPlayerContext
{
    public PlayerInput PlayerInput { get; }
    public CapsuleCollider CapsuleCollider { get; }
    public Transform Transform { get; }
    public SO_PlayerSettings Settings { get; }
    public PlayerState CurrentState { get; private set; }

    public PlayerContext(PlayerInput playerInput, CapsuleCollider capsuleCollider, Transform transform, SO_PlayerSettings settings)
    {
        this.PlayerInput = playerInput;
        this.CapsuleCollider = capsuleCollider;
        this.Transform = transform;
        this.Settings = settings;
    }

    // 状態をキャッシュしてGCを防止
    private readonly Dictionary<Type, PlayerState> _stateCache = new Dictionary<Type, PlayerState>();

    // 状態生成のカスタムファクトリ
    private readonly Dictionary<Type, Func<IPlayerContext, PlayerState>> _stateFactories = new Dictionary<Type, Func<IPlayerContext, PlayerState>>();

    public void RegisterStateFactory<T>(Func<IPlayerContext, PlayerState> factory) where T : PlayerState
    {
        _stateFactories[typeof(T)] = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// 状態遷移をまとめる
    /// ジェネリックを用いて、遷移先の状態を型で安全に指定できるようにする
    /// </summary>
    public void TransitionTo<T>() where T : PlayerState
    {
        var type = typeof(T);
        if (!_stateCache.ContainsKey(type))
        {
            // ファクトリが登録されていればそれを使う（テストやプールに対応）
            if (_stateFactories.TryGetValue(type, out var factory))
            {
                _stateCache[type] = factory(this);
            }
            else
            {
                // デフォルトはリフレクションで生成（既存コード互換）
                _stateCache[type] = (T)Activator.CreateInstance(type, this);
            }
        }

        CurrentState?.Exit();
        CurrentState = _stateCache[type];
        CurrentState.Enter();
    }
}
