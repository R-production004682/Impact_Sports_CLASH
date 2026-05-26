using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class TestEnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private GameObject ballPrefab;

    [Header("Movement")]
    [SerializeField, Tooltip("左右に移動する速度")]
    private float moveSpeed = 5.0f;

    [SerializeField, Tooltip("開始位置からの左右移動幅")]
    private float moveDistance = 3.0f;

    [SerializeField, Tooltip("開始時の向き。1=右、-1=左")]
    [Range(-1, 1)]
    private int initialDirection = 1;

    [Header("Throw")]
    [SerializeField, Tooltip("ボールを投げる間隔")]
    private float throwInterval = 2.0f;

    [SerializeField, Tooltip("投げる初速")]
    private float throwSpeed = 8.0f;

    [SerializeField, Tooltip("投球位置の高さ")]
    private float throwHeightOffset = 0.5f;

    [SerializeField, Tooltip("プレイヤーのどの高さを狙うか")]
    private float targetHeightOffset = 0.8f;

    [SerializeField, Tooltip("固定の山なり具合")]
    private float throwUpwardBias = 0.2f;

    [SerializeField, Tooltip("左右方向の制球ブレ角度（ブレてほしく無い場合は「0」）")]
    private float horizontalSpreadAngle = 5.0f;

    [SerializeField, Tooltip("上下方向の制球ブレ（ブレてほしく無い場合は「0」）")]
    private float verticalSpread = 0.08f;

    private Vector3 _startPosition;
    private float _leftX;
    private float _rightX;
    private float _direction;
    private float _throwTimer;
    private Transform _playerTransform;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Awake()
    {
        if (!TryResolveReferences())
        {
            enabled = false;
            return;
        }

        rb.freezeRotation = true;
    }

    private void Start()
    {
        _startPosition = transform.position;
        _direction = initialDirection == 0 ? 1f : Mathf.Sign(initialDirection);

        _leftX = _startPosition.x - Mathf.Abs(moveDistance);
        _rightX = _startPosition.x + Mathf.Abs(moveDistance);

        ApplyFacing();
        FindPlayer();
    }

    private void Update()
    {
        HandleThrowing();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    /// <summary>
    /// 必須コンポーネントがアタッチされているか確認し、なければ取得を試みる
    /// </summary>
    /// <returns></returns>
    private bool TryResolveReferences()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (capsuleCollider == null)
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
        }

        if (rb == null || capsuleCollider == null)
        {
            Debug.LogError($"{nameof(TestEnemyController)}: Rigidbody または CapsuleCollider が見つかりません。", this);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 標的（Player）を探す
    /// </summary>
    private void FindPlayer()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning($"{nameof(TestEnemyController)}: Player タグのオブジェクトが見つかりません。", this);
            return;
        }

        _playerTransform = playerObj.transform;
    }

    /// <summary>
    /// 開始位置を中心に moveDistance の範囲で左右に往復
    /// </summary>
    private void HandleMovement()
    {
        if (Mathf.Approximately(moveDistance, 0f) || Mathf.Approximately(moveSpeed, 0f))
        {
            return;
        }

        var nextPosition = rb.position + Vector3.right * _direction * moveSpeed * Time.fixedDeltaTime;

        if (_direction > 0f && nextPosition.x >= _rightX)
        {
            nextPosition.x = _rightX;
            _direction = -1f;
            ApplyFacing();
        }
        else if (_direction < 0f && nextPosition.x <= _leftX)
        {
            nextPosition.x = _leftX;
            _direction = 1f;
            ApplyFacing();
        }

        rb.MovePosition(nextPosition);
    }

    /// <summary>
    /// Playter を狙って一定間隔でボールを投げる
    /// あえて制球にブレを持たせる（本物っぽくするため）
    /// </summary>
    private void HandleThrowing()
    {
        if (ballPrefab == null || _playerTransform == null || throwInterval <= 0f)
        {
            return;
        }

        _throwTimer += Time.deltaTime;
        if (_throwTimer < throwInterval)
        {
            return;
        }

        _throwTimer = 0f;
        ThrowBall();
    }

    /// <summary>
    /// 投げる処理
    /// </summary>
    private void ThrowBall()
    {
        var spawnPosition = transform.position + Vector3.up * throwHeightOffset;
        var ballInstance = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);

        if (!ballInstance.TryGetComponent<Rigidbody>(out var ballRb))
        {
            Debug.LogError($"{nameof(TestEnemyController)}: {ballPrefab.name} に Rigidbody がありません。", ballPrefab);
            Destroy(ballInstance);
            return;
        }

        IgnoreSelfCollision(ballInstance);
        ballRb.linearVelocity = CalculateThrowDirection(spawnPosition) * throwSpeed;
    }

    /// <summary>
    /// 投げる方向を計算
    /// </summary>
    /// <param name="spawnPosition"></param>
    /// <returns></returns>
    private Vector3 CalculateThrowDirection(Vector3 spawnPosition)
    {
        var targetPosition = _playerTransform.position + Vector3.up * targetHeightOffset;
        var toTarget = targetPosition - spawnPosition;

        var horizontal = new Vector3(toTarget.x, 0f, toTarget.z);
        if (horizontal.sqrMagnitude < 0.001f)
        {
            horizontal = transform.forward;
        }

        horizontal.Normalize();

        var spreadYaw = Random.Range(-horizontalSpreadAngle, horizontalSpreadAngle);
        horizontal = Quaternion.AngleAxis(spreadYaw, Vector3.up) * horizontal;

        var upward = throwUpwardBias + Random.Range(-verticalSpread, verticalSpread);
        return (horizontal + Vector3.up * upward).normalized;
    }

    /// <summary>
    /// 自身が投げたボールと衝突しないようにする
    /// </summary>
    /// <param name="ballInstance"></param>
    private void IgnoreSelfCollision(GameObject ballInstance)
    {
        if (capsuleCollider == null)
        {
            return;
        }

        if (ballInstance.TryGetComponent<Collider>(out var ballCollider))
        {
            Physics.IgnoreCollision(ballCollider, capsuleCollider);
        }
    }

    /// <summary>
    /// オブジェクトの向きを移動方向に合わせて切り替える
    /// </summary>
    private void ApplyFacing()
    {
        var euler = transform.eulerAngles;
        euler.y = _direction > 0f ? 0f : 180f;
        transform.eulerAngles = euler;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var center = Application.isPlaying ? _startPosition : transform.position;
        var leftX = center.x - Mathf.Abs(moveDistance);
        var rightX = center.x + Mathf.Abs(moveDistance);

        var leftCenter = new Vector3(leftX, transform.position.y, transform.position.z);
        var rightCenter = new Vector3(rightX, transform.position.y, transform.position.z);

        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.DrawAAPolyLine(4f, leftCenter - Vector3.up * 0.1f, leftCenter + Vector3.up * 0.1f);
        UnityEditor.Handles.DrawAAPolyLine(4f, rightCenter - Vector3.up * 0.1f, rightCenter + Vector3.up * 0.1f);
        UnityEditor.Handles.DrawAAPolyLine(4f, leftCenter, rightCenter);
    }
#endif
}