using UnityEngine;

public class CharacterControllerPush : MonoBehaviour
{
    [SerializeField] private float pushForce = 3f;
    [SerializeField] private float maxMass = 50f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // No hay Rigidbody
        if (body == null)
            return;

        // Los objetos Kinematic no deben ser empujados por este sistema
        if (body.isKinematic)
            return;

        // Evitar empujar objetos al caer encima de ellos
        if (hit.moveDirection.y < -0.3f)
            return;

        // Evitar mover objetos absurdamente pesados
        if (body.mass > maxMass)
            return;

        Vector3 pushDirection = new Vector3(
            hit.moveDirection.x,
            0f,
            hit.moveDirection.z
        ).normalized;

        body.AddForce(
            pushDirection * pushForce,
            ForceMode.Impulse
        );
    }
}