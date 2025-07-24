using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentryController : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("How far the turret can see enemies")]
    [SerializeField] private float detectionRadius = 10f;
    [Tooltip("Which layers count as valid targets")]
    [SerializeField] private LayerMask targetMask;

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

    // find the closest enemy in range (or null)
    private void AcquireTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, targetMask);
        if (hits.Length == 0)
        {
            currentTarget = null;
            return;
        }

        Transform best = hits[0].transform;
        float bestDist = (best.position - transform.position).sqrMagnitude;

        for (int i = 1; i < hits.Length; i++)
        {
            float d = (hits[i].transform.position - transform.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = hits[i].transform;
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
