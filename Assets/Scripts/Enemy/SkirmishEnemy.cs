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

    private Vector3 lastKnownPlayerPosition;
    private float searchTimer = 0f;
    private float attackCooldownTimer = 0f;

    [Header("Enemy Status")]
    [SerializeField] private bool isAttacking = false;

    protected override void Awake()
    {
        base.Awake();
        agent.avoidancePriority = Random.Range(70, 90);
    }

    protected override void Update()
    {
        attackCooldownTimer -= Time.deltaTime;

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
        if (CanSeePlayer())
        {
            state = EnemyState.Chase;
            return;
        }

        base.HandlePathing();
        PlayPathingAnim();
    }

    private void HandleChase()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange && attackCooldownTimer <= 0f)
        {
            state = EnemyState.Attack;
            return;
        }

        PlayChaseAnim();

        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;
            agent.SetDestination(lastKnownPlayerPosition);

            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0f; // prevent tilting up/down
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
        else if (agent.remainingDistance <= tolerance)
        {
            state = EnemyState.Search;  
            searchTimer = searchDuration;
        }
    }

    private void HandleSearch()
    {
        searchTimer -= Time.deltaTime;
        transform.Rotate(Vector3.up * 90f * Time.deltaTime);

        PlaySearchAnim();

        if (CanSeePlayer())
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

    public IEnumerator PerformAttack()
    {
        isAttacking = true;
        agent.isStopped = true;

        // Enable hitbox
        attackHitbox.EnableHitbox();

        yield return new WaitForSeconds(attackLingerDuration);

        // Disable hitbox after attack window   
        attackHitbox.DisableHitbox();
    }


    private bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 toPlayer = (player.position - origin);
        float dist = toPlayer.magnitude;
        if (dist > viewRadius) return false;

        toPlayer.Normalize();
        float angleBetween = Vector3.Angle(transform.forward, toPlayer);
        if (angleBetween > viewAngle * 0.5f) return false;

        if (Physics.Raycast(origin, toPlayer, out var hit, viewRadius, obstacleMask | playerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform.root == transform) return false;
            return ((1 << hit.collider.gameObject.layer) & playerMask) != 0;
        }

        return false;
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

    public void EndAttackAnim()
    {
        SetAnimationState("Attack", false);
        attackCooldownTimer = attackCooldown;
        agent.isStopped = false;
        isAttacking = false;
        state = EnemyState.Chase;
    }
}
