using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] protected WeaponData weaponData;
    protected float nextFireTime = 0f;

    public abstract void Shoot();

    protected void PerformRaycast()
    {
        Ray ray = playerCamera.ViewportPointToRay(
        new Vector3(0.5f, 0.5f, 0.0f));
        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range,
        weaponData.hitLayers))
        {
            Debug.Log("Hit object: " + hit.collider.gameObject.name);
        }
    }
}

