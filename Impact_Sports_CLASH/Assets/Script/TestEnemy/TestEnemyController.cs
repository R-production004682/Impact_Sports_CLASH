using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class TestEnemyController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider capsuleCollider;

    [Header("移動設定")]
    [Tooltip("左右に移動する速度")]
    public float moveSpeed = 5.0f;
    [Tooltip("開始位置からの左右移動幅")]
    public float moveDistance = 3.0f;
    [Tooltip("開始時の向き。1=右、-1=左")]
    [Range(-1, 1)]
    public int initialDirection = 1;

    [Header("投球設定")]
    [Tooltip("投げる弾")]
    public GameObject ballObject;
    [Tooltip("ボールを投げる間隔")]
    public float throwInterval = 2.0f;
    [Tooltip("投げる初速")]
    public float throwForce = 8.0f;
    [Tooltip("投球時の高さ")]
    public float throwHeightOffset = 0.5f;

    private Vector3 _startPosition;
    private float _leftX;
    private float _rightX;
    private float _direction;

    private float _throwTimer = 0f;
    private Transform _playerTransform;

    private void Awake()
    {
        if (ValidateRequiredComponents())
        {
            enabled = false;
            Debug.LogError($"必須コンポーネントが不足しているため、{gameObject.name}のTestEnemyControllerを無効化しました。");
        }
    }

    private void Start()
    {
        // 弾当てられて転倒したら困るので、回転はさせない
        rb.freezeRotation = true;

        _startPosition = transform.position;
        _direction = initialDirection == 0 ? 1 : Mathf.Clamp(initialDirection, -1, 1); // 0は右向きとみなす

        // 移動範囲の算出
        _leftX = _startPosition.x - Mathf.Abs(moveDistance);
        _rightX = _startPosition.x + Mathf.Abs(moveDistance);

        ApplyFacing();

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        HandleThrowing();
    }

    private void FixedUpdate()
    {
        // 移動距離が0の場合やRigidbodyがアタッチされていない場合は移動処理をスキップ
        if (Mathf.Approximately(moveDistance, 0f) || rb == null)
        {
            return;
        }

        var delta = Vector3.right * _direction * moveSpeed * Time.fixedDeltaTime;
        var newPos = rb.position + delta;

        // 移動範囲チェックと方向転換
        if (_direction > 0 && newPos.x >= _rightX)
        {
            newPos.x = _rightX;
            _direction = -1;
            ApplyFacing();
        }
        else if (_direction < 0 && newPos.x <= _leftX)
        {
            newPos.x = _leftX;
            _direction = 1;
            ApplyFacing();
        }

        rb.MovePosition(newPos);
    }

    // 見た目の向きをX軸方向に合わせて反転（Y回転で反転）
    private void ApplyFacing()
    {
        var euler = transform.eulerAngles;
        euler.y = (_direction > 0) ? 0f : 180f;
        transform.eulerAngles = euler;
    }

    private void HandleThrowing()
    {
        // 必要条件チェック
        if (ballObject == null) 
        {
            return;
        }

        if (_playerTransform == null) 
        {
            return;
        }
        
        if (throwInterval <= 0f)
        {
            return;
        }

        _throwTimer += Time.deltaTime;
        if (_throwTimer >= throwInterval)
        {
            _throwTimer = 0f;
            ThrowBall();
        }
    }


    private void ThrowBall()
    {
        var spawnPos = transform.position + Vector3.up * throwHeightOffset;
        
        var ballInstance = Instantiate(ballObject, spawnPos, Quaternion.identity);
        var ballRb = ballInstance.GetComponent<Rigidbody>();
        if (ballRb == null)
        {
            Debug.LogError($"投げるオブジェクトにRigidbodyがアタッチされていません。{ballObject.name}にRigidbodyをアタッチしてください。");
            return;
        }

        // エネミー自身との衝突を無視する（生成直後の誤判定防止）
        if (ballInstance.TryGetComponent<Collider>(out var ballCollider))
        {
            Physics.IgnoreCollision(ballCollider, capsuleCollider);
        }

        // プレイヤー方向へ向けて初速を与える
        var targetPos = _playerTransform.position;
        var toTarget = targetPos - spawnPos;
        var direction = targetPos - spawnPos;
        var horizontal = new Vector3(direction.x, 0, direction.z).normalized;
        var dir = (horizontal + Vector3.up * 0.2f * horizontal.magnitude).normalized;

        ballRb.linearVelocity = dir * throwForce;
    }


    #region 必須コンポーネントがアタッチされているかのチェック
    private bool ValidateRequiredComponents()
    {
        var isValid = false;

        if (rb == null)
        {
            isValid = true;
        }

        if (capsuleCollider == null)
        {
            isValid = true;
        }

        if (isValid)
        {
            Debug.LogError($"必須コンポーネントがアタッチされていません。{gameObject.name}にRigidbodyとCapsuleColliderをアタッチしてください。");
        }

        return isValid;
    }
    #endregion

#if UNITY_EDITOR
    // シーンビューで移動範囲が見えるようにする
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            _startPosition = transform.position;
            _leftX = _startPosition.x - Mathf.Abs(moveDistance);
            _rightX = _startPosition.x + Mathf.Abs(moveDistance);
        }

        UnityEditor.Handles.color = Color.green;

        // 線の太さを指定（ピクセル単位）
        float thickness = 4f;

        Vector3 leftCenter = new Vector3(_leftX, transform.position.y, transform.position.z);
        Vector3 rightCenter = new Vector3(_rightX, transform.position.y, transform.position.z);

        UnityEditor.Handles.DrawAAPolyLine(thickness, leftCenter - Vector3.up * 0.1f, leftCenter + Vector3.up * 0.1f);
        UnityEditor.Handles.DrawAAPolyLine(thickness, rightCenter - Vector3.up * 0.1f, rightCenter + Vector3.up * 0.1f);
        UnityEditor.Handles.DrawAAPolyLine(thickness, leftCenter, rightCenter);
    }
#endif
}
