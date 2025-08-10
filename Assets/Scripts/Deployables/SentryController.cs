using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentryController : MonoBehaviour
{
    [Header("Rotation")]
    [Tooltip("Optional pivot to rotate (e.g. only yaw)")]
    [SerializeField] private Transform rotationPivot;
    [Tooltip("How quickly the turret turns")]
    [SerializeField] private float rotationSpeed = 5f;

    private Sentries sentry;            // your RaycastSentry subclass
    private Transform currentTarget;  // what we're aiming at

    void Awake()
    {
        sentry = GetComponent<Sentries>();
    }

    void Update()
    {
        AcquireTarget();
        if (currentTarget != null)
        {
            RotateToward(currentTarget.position);
            sentry.Shoot();      // uses your base PerformRaycast()
        }
    }

    // find the closest enemy in range
    private void AcquireTarget()
    {
        Collider[] inRange = Physics.OverlapSphere(transform.position, sentry.deployableData.range, sentry.GetHitLayer());
        if (inRange.Length == 0)
        {
            currentTarget = null;
            return;
        }

        Transform best = null;
        float bestDist = Mathf.Infinity;

        foreach (var hit in inRange)
        {
            Vector3 dir = hit.transform.position - transform.position;
            float dist = dir.magnitude;

            if (Physics.Raycast(transform.position, dir.normalized, out RaycastHit rayHit, dist, sentry.GetLosLayer()))
            {
                // Make sure the ray hits the enemy directly
                if (rayHit.transform == hit.transform)
                {
                    float sqrDist = dir.sqrMagnitude;
                    if (sqrDist < bestDist)
                    {
                        bestDist = sqrDist;
                        best = hit.transform;
                    }
                }
            }
        }

        currentTarget = best;
    }

    // smoothly turn the turret (or pivot) to face the target
    private void RotateToward(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position;
        dir.y = 0; // keep only horizontal rotation

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        if (rotationPivot != null)
        {
            rotationPivot.rotation = Quaternion.Slerp(
                rotationPivot.rotation,
                targetRot,
                Time.deltaTime * rotationSpeed
            );
        }
        else
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    // visualize detection radius in Scene view
    void OnDrawGizmosSelected()
    {
        if (sentry.deployableData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sentry.deployableData.range);
        }
    }
}
