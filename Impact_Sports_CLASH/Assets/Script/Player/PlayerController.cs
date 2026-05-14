using Algorithm;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private SO_PlayerSettings settings;

    private PlayerContext _playerContext;

    private void Awake()
    {
        if (!ValidateRequiredComponents())
        {
            enabled = false;
            Debug.LogError("PlayerController: 必須コンポーネントが不足しています。", this);
            return;
        }
        _playerContext = new PlayerContext(playerInput, capsuleCollider, transform, settings);
    }

    private void Start()
    {
        _playerContext.TransitionTo<PlayerIdleState>();
    }

    private void Update()
    {
        _playerContext.CurrentState?.Execute();
    }

    // TODO : 将来的に使うので一旦コメントアウト
    //private void FixedUpdate()
    //{
    //    _playerContext.CurrentState?.FixedExecute();
    //}

    #region 必須コンポーネントがアタッチされているかのチェック
    private bool ValidateRequiredComponents()
    {
        bool isValid = true;
#if UNITY_EDITOR
        // 必須コンポーネントチェック
        isValid = ComponentValidator.ValidateAndLogRequired(
            this,
            playerInput,
            capsuleCollider,
            settings
        );

        if (!isValid)
        {
            return false;
        }
#endif
        return isValid;
    }
    #endregion
}