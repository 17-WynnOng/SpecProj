using UnityEngine;

public class Sentries : Deployable
{
    [SerializeField] private Transform firePoint;
    private float nextFireTime;

    protected virtual Ray GetFireRay()
    {
        return new Ray(firePoint.position, firePoint.forward);
    }

    public void Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + deployableData.fireRate;
            PerformRaycast();
        }
    }

    private void PerformRaycast()
    {
        Ray ray = GetFireRay();
        if (Physics.Raycast(ray, out var hit, deployableData.range, deployableData.hitLayers))
        {
            if (hit.collider.TryGetComponent<Damageable>(out var d))
                d.TakeDamage(deployableData.damage);
        }
    }
}
