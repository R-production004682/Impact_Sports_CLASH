using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettings")]
public class SO_PlayerSettings : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("移動速度")] public float MoveSpeed = 5f;
    [Tooltip("移動最大速度")] public float MaxMoveSpeed = 10f;
    [Tooltip("摩擦")] public float GrandFriction = 0.1f;
    [Tooltip("加速度")] public float Acceleration = 10f;

    [Header("Shooting")]
    [Tooltip("投げる力")] public float ThrowForce = 20f;
    [Tooltip("クールダウン時間")] public float CooldownTime = 1.0f;
    [Tooltip("ボールのプレハブ")] public GameObject BallPrefab;

    [Header("Ball Hold")]
    [Tooltip("最大所持数")] public int MaxHoldCount = 2;
    [Tooltip("初期所持数")] public int InitialBallCount = 2;
}
