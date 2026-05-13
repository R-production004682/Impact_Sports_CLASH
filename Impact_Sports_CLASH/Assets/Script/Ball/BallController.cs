using UnityEngine;

/// <summary>
/// TODO : テストで作成したものなので、必要に応じて機能追加やリファクタリングを行うこと
/// </summary>
public class BallController : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private bool destroyOnCollision = false;

    private float now = 0f;

    private void Update()
    {
        now += Time.deltaTime;
        if (now >= lifetime)
        {
            now = 0f;
            Destroy(gameObject);
        }

    }

    private void OnCollisionEnter(Collision other)
    {
        if (!destroyOnCollision) return;
        Destroy(gameObject);
    }
}