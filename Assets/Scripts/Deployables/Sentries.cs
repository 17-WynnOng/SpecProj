using UnityEngine;
using System.Collections;

public class Sentries : Deployable
{
    [Header("Firing")]
    [SerializeField] private Transform firePoint;
    private float nextFireTime;

    [Header("Tracer & Muzzle Flash")]
    [SerializeField] private TrailRenderer trailPrefab;
    [SerializeField] private int shotsForTracer = 2;

    private int shotCounter = 0;

    private int currentMag;
    private bool isReloading;

    [SerializeField] private Renderer tintRenderer;   // assign in Inspector
    private Material tintMat;
    private Color baseColor;
    private Coroutine reloadTintRoutine;


    protected virtual void Start()
    {
        // Treat one "magazine" as equal to maxAmmo (as requested).
        currentMag = deployableData.magazineSize;
        isReloading = false;

        if (tintRenderer != null)
        {
            tintMat = tintRenderer.material;  // creates a per-instance material
            baseColor = tintMat.color;

            SetTintFromAmmo(); // set initial target
        }
    }

    protected virtual Ray GetFireRay()
    {
        return new Ray(firePoint.position, firePoint.forward);
    }
    public LayerMask GetHitLayer()
    {
        return deployableData.hitLayers;
    }

    public LayerMask GetLosLayer()
    {
        return deployableData.losLayers;
    }

    public void Shoot()
    {
        if (isReloading)
            return;

        // If empty, start reload and bail
        if (currentMag <= 0)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }

        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + deployableData.fireRate;
            currentMag--;
            PerformRaycast();
            SetTintFromAmmo();
        }
    }

    public void PerformRaycast()
    {
        shotCounter++;

        Ray ray = GetFireRay();
        Vector3 hitPoint = ray.origin + ray.direction * deployableData.range;

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

        if (shotCounter >= shotsForTracer)
        {
            SpawnTrail(firePoint.position, hitPoint);
            shotCounter = 0;
        }
    }

    private IEnumerator ReloadRoutine()
    {
        if (isReloading) yield break;
        isReloading = true;

        currentMag = deployableData.magazineSize;

        float duration = Mathf.Max(0.01f, deployableData.reloadTime);
        if (tintMat != null)
        {
            Color from = tintMat.color;   // current (likely red-ish)
            Color to = baseColor;       // back to base

            // Cancel any previous tint lerp
            if (reloadTintRoutine != null) StopCoroutine(reloadTintRoutine);
            reloadTintRoutine = StartCoroutine(LerpTint(from, to, duration));
        }

        yield return new WaitForSeconds(duration);

        isReloading = false;
        SetTintFromAmmo();
    }

    private void SpawnTrail(Vector3 start, Vector3 end)
    {
        TrailRenderer trail = Instantiate(trailPrefab, start, Quaternion.identity);
        StartCoroutine(AnimateTrail(trail, start, end));
    }

    private IEnumerator AnimateTrail(TrailRenderer trail, Vector3 start, Vector3 end)
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

    private void SetTintFromAmmo()
    {
        if (tintMat == null) return;

        // 0 = base color (full), 1 = red (empty)
        float pct = deployableData.magazineSize > 0
            ? (float)currentMag / deployableData.magazineSize
            : 1f;

        tintMat.color = Color.Lerp(baseColor, Color.red, 1f - pct);
    }

    private IEnumerator LerpTint(Color from, Color to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float a = (dur <= 0f) ? 1f : t / dur;

            if (tintMat != null)
                tintMat.color = Color.Lerp(from, to, a);
            // or: tintMat.SetColor("_BaseColor", Color.Lerp(from, to, a));

            yield return null;
        }

        if (tintMat != null)
            tintMat.color = to;

        reloadTintRoutine = null;
    }
}
