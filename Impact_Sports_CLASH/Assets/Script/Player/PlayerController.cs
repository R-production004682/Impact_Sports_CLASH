using Algorithm;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private SO_PlayerSettings settings;

    private PlayerContext _playerContext;

    private void Awake()
    {
        // TODO : 未割当のコンポーネントが無いように、初期値や補完の仕組みを入れる？

        if (!ValidateRequiredComponents())
        {
            enabled = false;
            Debug.LogError("PlayerController: 必須コンポーネントが不足しています。", this);
            return;
        }

        _playerContext = new PlayerContext(
            playerInput: playerInput,
            capsuleCollider: capsuleCollider, 
            rb: rb, 
            transform: transform,
            settings: settings);

        _playerContext.Rigidbody.freezeRotation = true;
    }

    private void Start()
    {
        _playerContext.TransitionTo<PlayerIdleState>();
    }

    private void Update()
    {
        _playerContext.CurrentState?.Execute();
    }

    private void FixedUpdate()
    {
        _playerContext.CurrentState?.FixedExecute();
    }

    #region 必須コンポーネントがアタッチされているかのチェック
    private bool ValidateRequiredComponents()
    {
        // 必須コンポーネントチェック
        return ComponentValidator.ValidateAndLogRequired(
            this,
            playerInput,
            capsuleCollider,
            rb,
            settings
        );
    }
    #endregion
}