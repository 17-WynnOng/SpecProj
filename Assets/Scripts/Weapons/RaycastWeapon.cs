using System.Runtime.CompilerServices;
using UnityEngine;
public class RaycastWeapon : Weapon
{
    [Header("Tracer & Muzzle Flash")]
    [SerializeField] private Transform firePoint; // assign this to the barrel of the gun
    [SerializeField] private TrailRenderer trailPrefab;
    [SerializeField] private int shotsForTracer = 4;

    public override bool Shoot()
    {
        if (isReloading || currentMag <= 0 || Time.time < nextFireTime)
            return false;

        nextFireTime = Time.time + weaponData.fireRate;

        PerformRaycast();
        if (animator != null)
            animator.SetTrigger("Shoot");

        currentMag--;
        UIManager.Instance.UpdateAmmoUI(currentMag, currentReserve);
        AudioManager.Instance.PlayOneShotSFXByName("arFire");

        return true;
    }

    public override void PerformRaycast()
    {
        shotCounter++;

        Ray ray = GetFireRay();
        Vector3 hitPoint = ray.origin + ray.direction * weaponData.range;

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

        if (shotCounter >= shotsForTracer)
        {
            SpawnTrail(firePoint.position, hitPoint);
            shotCounter = 0;
        }
    }

    private void SpawnTrail(Vector3 start, Vector3 end)
    {
        TrailRenderer trail = Instantiate(trailPrefab, start, Quaternion.identity);
        StartCoroutine(AnimateTrail(trail, start, end));
    }

    private System.Collections.IEnumerator AnimateTrail(TrailRenderer trail, Vector3 start, Vector3 end)
    {
        float time = 0f;
        float duration = 0.05f; // how fast it moves visually

        while (time < duration)
        {
            trail.transform.position = Vector3.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        trail.transform.position = end;
        yield return new WaitForSeconds(trail.time); // wait for it to fade
        Destroy(trail.gameObject);
    }
}
