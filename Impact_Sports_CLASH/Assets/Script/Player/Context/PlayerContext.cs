using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerContext
{
    public PlayerInput PlayerInput { get; }
    public CapsuleCollider CapsuleCollider { get; }
    public Transform Transform { get; }
    public SO_PlayerSettings Settings { get; }
    public PlayerState CurrentState { get; private set; }

    /// <summary>
    /// Playerの状態やコンポーネントへのアクセスをまとめるコンテナ
    /// </summary>
    /// <param name="playerInput"></param>
    /// <param name="capsuleCollider"></param>
    /// <param name="transform"></param>
    /// <param name="settings"></param>
    public PlayerContext(PlayerInput playerInput, CapsuleCollider capsuleCollider, Transform transform, SO_PlayerSettings settings)
    {
        this.PlayerInput = playerInput;
        this.CapsuleCollider = capsuleCollider;
        this.Transform = transform;
        this.Settings = settings;
    }

    // 状態をキャッシュしてGCを防止
    private readonly Dictionary<Type, PlayerState> _stateCache = new();

    /// <summary>
    /// 状態遷移をまとめる
    /// ジェネリックを用いて、遷移先の状態を型で安全に指定できるようにする
    /// </summary>
    /// <typeparam name="T">遷移先の状態の型</typeparam>
    public void TransitionTo<T>() where T : PlayerState
    {
        var type = typeof(T);
        if (!_stateCache.ContainsKey(type))
        {
            // 初めてのStateなら生成してキャッシュ
            _stateCache[type] = (T)Activator.CreateInstance(type, this);
        }

        CurrentState?.Exit();
        CurrentState = _stateCache[type];
        CurrentState.Enter();
    }
}
