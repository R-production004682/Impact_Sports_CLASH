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
        _playerContext = new PlayerContext(playerInput, capsuleCollider, transform, settings);
    }

    private void Start()
    {
        // ジェネリクスを使った遷移で型安全に
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
}