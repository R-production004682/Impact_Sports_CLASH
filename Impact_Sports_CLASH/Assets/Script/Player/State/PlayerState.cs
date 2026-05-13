using UnityEngine;
using UnityEngine.InputSystem;
using Clash.Constants;

public abstract class PlayerState
{
    protected PlayerContext Context;
    private readonly InputAction _shootAction;
    private readonly InputAction _moveAction;

    protected PlayerState(PlayerContext context)
    {
        Context = context;

        // アクションを取得
        _shootAction = context.PlayerInput.actions[InputActionId.Shoot.ToIdName()];
        _moveAction = context.PlayerInput.actions[InputActionId.Move.ToIdName()];
    }

    /// <summary> 状態に入ったときに呼ばれる(Startメソッドと同じ) </summary>
    public virtual void Enter() { }

    /// <summary> 状態が更新されるときに呼ばれる(Updateメソッドと同じ) </summary>
    public virtual void Execute() { }

    /// <summary> 状態が固定更新されるときに呼ばれる(FixedUpdateメソッドと同じ) </summary>
    public virtual void FixedExecute() { }

    /// <summary> 状態から出るときに呼ばれる(OnDestroyメソッドと同じ) </summary>
    public virtual void Exit() { }

    // 共通で使う入力判定
    protected bool IsShotTriggered() => _shootAction.triggered;
    protected Vector2 GetMoveInput() => _moveAction.ReadValue<Vector2>();
}