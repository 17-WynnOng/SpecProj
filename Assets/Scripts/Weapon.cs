using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] public WeaponData weaponData;
    protected float nextFireTime = 0f;

    public void Initialize(WeaponData data, Camera cam = null)
    {
        weaponData = data;
        playerCamera = cam;  // null for turret
    }

    // hook for subclasses or setup code to define origin+direction
    protected virtual Ray GetFireRay()
    {
        // default for player weapons:
        return playerCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
    }

    public abstract void Shoot();

    public void PerformRaycast()
    {
        Ray ray = GetFireRay();
        if (Physics.Raycast(ray, out var hit, weaponData.range, weaponData.hitLayers))
        {
            if (hit.collider.TryGetComponent<Damageable>(out var d))
                d.TakeDamage(weaponData.damage);
        }
    }
}

