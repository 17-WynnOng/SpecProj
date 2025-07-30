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

    public LayerMask GetHitLayer()
    {
        return deployableData.hitLayers;
    }

    public LayerMask GetLosLayer()
    {
        return deployableData.losLayers;
    }

    public void PerformRaycast()
    {
        Ray ray = GetFireRay();
        if (Physics.Raycast(ray, out var hit, deployableData.range))
        {
            // Stop at first object hit
            if (((1 << hit.collider.gameObject.layer) & deployableData.hitLayers) != 0)
            {
                if (hit.collider.TryGetComponent<Damageable>(out var d))
                {
                    d.TakeDamage(deployableData.damage);
                }
            }
        }
    }
}
