using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatHitbox : MonoBehaviour
{
    private float damage;
    private float hitStun;
    private float knockbackForce;

    private FighterEntity owner;
    private Collider myCollider;
    private List<IDamageable> victims = new List<IDamageable>();

    private AttackData attackData;
    private ActionData actionData;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        owner = GetComponentInParent<FighterEntity>();

        if (myCollider != null)
            myCollider.enabled = false;
    }

    public void EnableHitbox(AttackData attack, ActionData action = null)
    {
        if (attack == null)
        {
            Debug.LogError($"{gameObject.name}: Se intentó activar la hitbox sin AttackData.", this);
            return;
        }

        attackData = attack;
        actionData = action;

        damage = attack.damage;
        hitStun = attack.hitStun;
        knockbackForce = attack.knockbackForce;

        victims.Clear();

        if (myCollider != null)
            myCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        if (myCollider != null)
            myCollider.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (owner != null && other.gameObject == owner.gameObject)
            return;

        IDamageable target = other.GetComponent<IDamageable>();
        if (target == null || victims.Contains(target))
            return;

        victims.Add(target);

        Vector3 hitPoint = myCollider.ClosestPoint(other.bounds.center);

        if (attackData != null)
        {
            string actionName = actionData != null ? actionData.actionName : "Acción Desconocida";

            Debug.Log($"<color=yellow>HIT:</color> {owner.gameObject.name} -> {other.gameObject.name}");

            if (attackData.hitParticle != null)
            {
                Debug.Log(
                    $"<color=lime>CREANDO PARTICULA</color> " +
                    $"Ataque: {actionName} | " +
                    $"Prefab: {attackData.hitParticle.name} | " +
                    $"Posición: {hitPoint}"
                );

                Instantiate(attackData.hitParticle, hitPoint, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning($"El ataque '{actionName}' no tiene asignado un hitParticle en su AttackData.");
            }

            if (attackData.hitSound != null)
            {
                AudioSource.PlayClipAtPoint(attackData.hitSound, hitPoint);
            }
        }
        else
        {
            Debug.LogError($"attackData es NULL en la Hitbox de {owner.gameObject.name}");
        }

        // 3. APLICAR DAÑO
        target.TakeDamage(damage, hitStun);
    }
}
