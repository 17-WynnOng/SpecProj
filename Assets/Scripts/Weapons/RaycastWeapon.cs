using System.Runtime.CompilerServices;
using UnityEngine;
public class RaycastWeapon : Weapon
{
    public override bool Shoot()
    {
        if (isReloading || currentMag <= 0 || Time.time < nextFireTime)
            return false;

        nextFireTime = Time.time + weaponData.fireRate;

        PerformRaycast(); // damage logic here
        if (animator != null)
            animator.SetTrigger("Shoot");

        currentMag--;
        UIManager.Instance.UpdateAmmoUI(currentMag, currentReserve); // optional
        return true;
    }

    public override void PerformRaycast()
    {
        Ray ray = GetFireRay();
        if (Physics.Raycast(ray, out var hit, weaponData.range))
        {
            // Stop at first object hit
            if (((1 << hit.collider.gameObject.layer) & weaponData.hitLayers) != 0)
            {
                if (hit.collider.TryGetComponent<Damageable>(out var d))
                {
                    d.TakeDamage(weaponData.damage);
                    d.IfDamagedByPlayer();
                }
            }
        }
    }
}
