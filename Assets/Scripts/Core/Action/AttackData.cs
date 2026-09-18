using UnityEngine;

[CreateAssetMenu(
    fileName = "New Attack",
    menuName = "Action/Attack"
)]
public class AttackData : ScriptableObject
{
    [Header("Combate")]
    public float damage = 0f;
    public float hitStun = 0f;
    public float knockbackForce = 0f;

    [Header("VFX & SFX")]
    public GameObject hitParticle;
    public AudioClip hitSound;
}
