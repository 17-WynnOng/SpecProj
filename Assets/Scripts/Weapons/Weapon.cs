using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] public WeaponData weaponData;
    protected float nextFireTime = 0f;

    public int currentMag;
    public int currentReserve;
    public bool isReloading = false;

    [SerializeField] public Animator animator;

    public void Initialize(WeaponData data, Camera cam = null)
    {
        weaponData = data;
        playerCamera = cam;  // null for turret
        currentMag = weaponData.magazineSize;
        currentReserve = weaponData.maxAmmo;
    }

    // hook for subclasses or setup code to define origin+direction
    protected virtual Ray GetFireRay()
    {
        // default for player weapons:
        return playerCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
    }

    public abstract bool Shoot();

    public void Reload()
    {
        if (isReloading || currentMag == weaponData.magazineSize || currentReserve == 0)
            return;

        StartReloadAnim();
    }

    public void StartReloadAnim()
    {
        isReloading = true;
        if (animator != null)
        {
            animator.SetBool("Reload", true);
        }
    }

    public void EndReloadAnim()
    {
        if (animator != null)
        {
            animator.SetBool("Reload", false);
        }
        int needed = weaponData.magazineSize - currentMag;
        int toLoad = Mathf.Min(needed, currentReserve);
        currentMag += toLoad;
        currentReserve -= toLoad;

        UIManager.Instance.UpdateAmmoUI(currentMag, currentReserve);

        isReloading = false;
    }

    public virtual void PerformRaycast()
    {
        Ray ray = GetFireRay();
        if (Physics.Raycast(ray, out var hit, weaponData.range, weaponData.hitLayers))
        {
            if (hit.collider.TryGetComponent<Damageable>(out var d))
                d.TakeDamage(weaponData.damage);
        }
    }
}

