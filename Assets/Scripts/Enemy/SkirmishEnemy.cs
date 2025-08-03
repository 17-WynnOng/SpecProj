using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

public class SkirmishEnemy : EnemyAI
{
    [Header("Vision")]
    public float viewRadius;
    public float viewAngle;
    public LayerMask playerMask, obstacleMask;
    public float searchDuration = 3f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackLingerDuration = 0.5f;
    [SerializeField] private EnemyAttackHitbox attackHitbox;

    [Header("Chase Settings")]
    [SerializeField] private float defaultSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 5.5f;

    private Vector3 lastKnownPlayerPosition;
    private float searchTimer = 0f;
    private float attackCooldownTimer = 0f;

    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 3f;
    [SerializeField] private float dashSpeed = 10f;

    private bool canDash = true;
    private bool isDashing = false;

    [Header("Enemy Status")]
    [SerializeField] private bool isAttacking = false;

    private float visionTimer = 0f;
    [SerializeField] private float reactionTime = 0.1f; // how often to run CanSeePlayer
    private bool savedCanSeePlayer = false;

    protected override void Awake()
    {
        base.Awake();
        agent.avoidancePriority = Random.Range(70, 90);
        visionTimer = Random.Range(0f, reactionTime);
    }

    protected override void Update()
    {
        attackCooldownTimer -= Time.deltaTime;
        visionTimer -= Time.deltaTime;

        switch (state)
        {
            case EnemyState.Patrol:
                HandlePathing();
                break;

            case EnemyState.Chase:
                HandleChase();
                break;

            case EnemyState.Search:
                HandleSearch();
                break;
            case EnemyState.Attack:
                HandleAttack();
                break;
        }
    }

    protected override void HandlePathing()
    {
        agent.speed = defaultSpeed;

        if (CanSeePlayerOptimised())
        {
            state = EnemyState.Chase;
            return;
        }

        base.HandlePathing();
        PlayPathingAnim();
    }

    private void HandleChase()
    {
        agent.speed = chaseSpeed;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange && attackCooldownTimer <= 0f)
        {
            state = EnemyState.Attack;
            return;
        }

        PlayChaseAnim();

        if (agent.enabled == true)
        {
            if (CanSeePlayerOptimised())
            {
                lastKnownPlayerPosition = player.position;

                Vector3 lookDirection = player.position - transform.position;
                lookDirection.y = 0f; // prevent tilting up/down
                if (lookDirection.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
                }

                // Only move if NavMeshAgent is active
                if (agent.enabled)
                {
                    agent.SetDestination(lastKnownPlayerPosition);
                }
            }
            else if (agent.enabled && agent.remainingDistance <= tolerance)
            {
                state = EnemyState.Search;
                searchTimer = searchDuration;
            }
        }
    }

    private void HandleSearch()
    {
        agent.speed = defaultSpeed;

        searchTimer -= Time.deltaTime;
        transform.Rotate(Vector3.up * 90f * Time.deltaTime);

        PlaySearchAnim();

        if (CanSeePlayerOptimised())
        {
            state = EnemyState.Chase;
            return;
        }

        if (searchTimer <= 0f)
        {
            state = EnemyState.Patrol;
            currentIndex = GetClosestWaypointIndex();
            agent.SetDestination(path[currentIndex].position);
        }
    }

    private void HandleAttack()
    {
        if (!isAttacking)
        {
            attackHitbox.SetDamage(attackDamage);
            PlayAttackAnim();
        }
    }

    private IEnumerator PerformRandomDash()
    {
        canDash = false;
        isDashing = true;
        agent.enabled = false;  

        float minClearDistance = 2f; // how much space is needed to dash
        float maxDashCheck = dashDistance + 0.6f; //max dash distance
        float buffer = 0.1f;

        float rightMaxDist = maxDashCheck;
        float leftMaxDist = maxDashCheck;

        // Cast to the right
        if (Physics.Raycast(transform.position, transform.right, out RaycastHit hitRight, dashDistance, LayerMask.GetMask("Environment")))
        {
            rightMaxDist = hitRight.distance - buffer;
        }

        // Cast to the left
        if (Physics.Raycast(transform.position, -transform.right, out RaycastHit hitLeft, dashDistance, LayerMask.GetMask("Environment")))
        {
            leftMaxDist = hitLeft.distance - buffer;
        }

        // Decide direction
        Vector3 dashDir;
        float actualDashDistance;

        if (rightMaxDist >= minClearDistance && leftMaxDist >= minClearDistance)
        {
            // Both directions clear → pick random
            bool dashRight = Random.value < 0.5f;
            dashDir = dashRight ? transform.right : -transform.right;
            actualDashDistance = dashRight ? Random.Range(minClearDistance, rightMaxDist) : Random.Range(minClearDistance, leftMaxDist);
            DashAnim(dashRight ? "DashRight" : "DashLeft", true);
        }
        else if (rightMaxDist >= minClearDistance)
        {
            dashDir = transform.right;
            actualDashDistance = Random.Range(minClearDistance, rightMaxDist);
            DashAnim("DashRight", true);
        }
        else if (leftMaxDist >= minClearDistance)
        {
            dashDir = -transform.right;
            actualDashDistance = Random.Range(minClearDistance, leftMaxDist);
            DashAnim("DashRight", false);
        }
        else
        {
            // No valid direction → cancel dash
            agent.enabled = true;
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
            isDashing = false;
            yield break;
        }

        Vector3 start = transform.position;
        Vector3 end = start + dashDir * actualDashDistance;
        float dashDuration = actualDashDistance / dashSpeed;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0f; // prevent tilting up/down
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }

            transform.position = Vector3.Lerp(start, end, elapsed / dashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        agent.enabled = true;
        isDashing = false;
        agent.SetDestination(lastKnownPlayerPosition);

        DashAnim("DashRight", false);
        DashAnim("DashLeft", false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }


    public IEnumerator PerformAttack()
    {
        if (agent.enabled)
        {
            isAttacking = true;
            agent.isStopped = true;

            // Enable hitbox
            attackHitbox.EnableHitbox();

            yield return new WaitForSeconds(attackLingerDuration);

            // Disable hitbox after attack window   
            attackHitbox.DisableHitbox();
        }
    }

    private bool CanSeePlayerOptimised()
    {
        if (visionTimer <= 0f)
        {
            visionTimer = reactionTime;

            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 toPlayer = player.position - origin;

            //Distance check before raycast
            float dist = toPlayer.magnitude;
            if (dist > viewRadius)
            {
                savedCanSeePlayer = false;
                return false;
            }

            //Angle check if player within distance
            toPlayer.Normalize();
            float angleBetween = Vector3.Angle(transform.forward, toPlayer);
            if (angleBetween > viewAngle * 0.5f)
            {
                savedCanSeePlayer = false;
                return false;
            }

            //Only raycast if distance and angle are valid
            if (Physics.Raycast(origin, toPlayer, out var hit, viewRadius, obstacleMask | playerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform.root == transform)
                {
                    savedCanSeePlayer = false;
                    return false;
                }

                savedCanSeePlayer = ((1 << hit.collider.gameObject.layer) & playerMask) != 0;
            }
        }

        return savedCanSeePlayer;
    }

    public override void IfDamagedByPlayer()
    {
        if (state != EnemyState.Chase && state != EnemyState.Attack)
        {
            state = EnemyState.Chase;
            lastKnownPlayerPosition = player.position;
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.SetDestination(lastKnownPlayerPosition);
            }
        }
        else if (state == EnemyState.Chase && canDash && !isDashing)
        {
            // 40% chance to dodge when hit during chase
            if (Random.value < 0.4f)
            {
                StartCoroutine(PerformRandomDash());
            }
        }
    }

    private void SetAnimationState(string parameter, bool state)
    {
        if (animator != null)
            animator.SetBool(parameter, state);
    }

    private void PlayChaseAnim()
    {
        SetAnimationState("Chasing", true);
        SetAnimationState("Search", false);
    }

    private void PlaySearchAnim()
    {
        SetAnimationState("Chasing", false);
        SetAnimationState("Search", true);
    }

    private void PlayPathingAnim()
    {
        SetAnimationState("Chasing", false);
        SetAnimationState("Search", false);
    }

    private void PlayAttackAnim()
    {
        SetAnimationState("Attack", true);
    }

    private void DashAnim(string direction, bool state)
    {
        SetAnimationState(direction, state);
    }

    public void EndAttackAnim()
    {
        SetAnimationState("Attack", false);
        attackCooldownTimer = attackCooldown;
        agent.isStopped = false;
        isAttacking = false;
        state = EnemyState.Chase;
    }
}
