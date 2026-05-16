using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettings")]
public class SO_PlayerSettings : ScriptableObject
{
    [Header("Movement")]
    public float MoveSpeed = 5f;

    [Header("Shooting")]
    public float ThrowForce = 20f;
    public float CooldownTime = 1.0f;
    public GameObject BallPrefab;
}
