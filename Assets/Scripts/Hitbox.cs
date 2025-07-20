using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    private float damage;
    [SerializeField] private Collider hitboxCollider;
    [SerializeField] private LayerMask damageableLayers;

    public void EnableHitbox()
    {
        hitboxCollider.enabled = true;
    }
    public void DisableHitbox()
    {
        hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & damageableLayers) == 0) return;

        if (other.TryGetComponent<Damageable>(out var dmg))
        {
            dmg.TakeDamage(damage);
        }
    }

    public void SetDamage(float amount)
    {
        damage = amount;
    }
}