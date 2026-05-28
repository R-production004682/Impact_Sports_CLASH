using Algorithm;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject releasePoint;
    [SerializeField] private GameObject holdPoint;
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
            settings: settings,
            releasePointTransform: releasePoint.transform,
            holdPointTransform: holdPoint.transform);

        _playerContext.Rigidbody.freezeRotation = true;
    }

    private void Start()
    {
        _playerContext.BallHolder.InitializeBalls(settings.InitialBallCount);
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
            settings,
            releasePoint,
            holdPoint
        );
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (settings == null)
        {
            return;
        }

        var center = transform.position + transform.forward * settings.CatchForwardOffset;
        var radius = settings.CatchRadius;

        // キャッチウィンドウ中は緑、それ以外は黄色
        var isCatching = Application.isPlaying && _playerContext?.CurrentState is PlayerCatchState;

        Gizmos.color = isCatching ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(center, radius);

        // 半透明の塗り球
        var fillColor = isCatching
            ? new Color(0f, 1f, 0f, 0.15f)
            : new Color(1f, 1f, 0f, 0.08f);

        Gizmos.color = fillColor;
        Gizmos.DrawSphere(center, radius);
        Gizmos.color = isCatching ? Color.green : Color.yellow;
        Gizmos.DrawLine(transform.position, center);

        // ラベル表示
        var style = new GUIStyle
        {
            normal = { textColor = isCatching ? Color.green : Color.yellow },
            fontSize = 11,
            fontStyle = FontStyle.Bold
        };
        var label = isCatching ? "● CATCH ACTIVE" : "○ Catch Range";
        UnityEditor.Handles.Label(center + Vector3.up * (radius + 0.15f), label, style);
    }
#endif
}