using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkirmishEnemy : EnemyAI
{
    [Header("Vision")]
    public float viewRadius;
    public float viewAngle;
    public LayerMask playerMask, obstacleMask;
    public float tolerance = 0.3f;

    [SerializeField]private bool isChasing;

    protected override bool ShouldChasePlayer()
    {
        return CanSeePlayer();
    }

    protected override void ChasePlayer()
    {
        isChasing = true;
        agent.SetDestination(player.position);
    }

    protected override void PatrolPath()
    {
        if (isChasing)
        {
            // lost the player?
            isChasing = false;
            if (currentIndex < path.Length)
                agent.SetDestination(path[currentIndex].position);
            return;
        }

        if (agent.pathPending || agent.remainingDistance > tolerance) return;

        currentIndex++;
        if (currentIndex < path.Length)
            agent.SetDestination(path[currentIndex].position);
        else
            Destroy(gameObject);
    }

    private bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 toPlayer = (player.position - origin);
        float dist = toPlayer.magnitude;

        // 1) Distance check
        if (dist > viewRadius) return false;

        toPlayer.Normalize();

        // 2) FOV angle check
        float angleBetween = Vector3.Angle(transform.forward, toPlayer);
        if (angleBetween > viewAngle * 0.5f) return false;

        // —— DEBUG VISUALIZATION ——
        // Draw a red line in Scene view showing exactly where the ray is going
        Debug.DrawRay(origin, toPlayer * viewRadius, Color.red, 0.1f);

        // 3) Raycast (no mask) just to see what we hit first
        if (Physics.Raycast(origin, toPlayer, out var hitInfo, viewRadius))
        {
            Debug.Log($"Raycast hit: {hitInfo.collider.name} (layer {hitInfo.collider.gameObject.layer})");

            // now apply your layer‐mask logic:
            int mask = obstacleMask | playerMask;
            if (((1 << hitInfo.collider.gameObject.layer) & playerMask) != 0)
            {
                Debug.Log("--> First hit is the player, so we can see them!");
                return true;
            }
            else
            {
                Debug.Log("--> Hit something else first, blocking view.");
                return false;
            }
        }
        else
        {
            Debug.Log("Raycast hit nothing at all!");
            return false;
        }
    }
}
