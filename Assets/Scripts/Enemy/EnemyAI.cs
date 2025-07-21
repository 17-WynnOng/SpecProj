using UnityEngine.AI;
using UnityEngine;

public enum EnemyState
{
    Patrol,
    Chase,
    Search,
    Attack
}

public abstract class EnemyAI : Damageable
{
    protected NavMeshAgent agent;

    [Header("Path Finding")]
    [SerializeField] protected Transform[] path;
    [SerializeField] protected Transform player;
    [SerializeField] protected float tolerance = 0.3f;
    protected int currentIndex;

    protected EnemyState state = EnemyState.Patrol;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        agent.avoidancePriority = Random.Range(30, 70);
        int enemyLayerIndex = LayerMask.NameToLayer("Enemy");
        Physics.IgnoreLayerCollision(enemyLayerIndex, enemyLayerIndex);
    }

    protected override void Start()
    {
        base.Start();
        path = GameManager.Instance.levelPath.waypoints;
        currentIndex = 0;
        if (path.Length > 0)
            agent.SetDestination(path[0].position);
    }

    protected virtual void HandlePathing()
    {
        if (agent.pathPending || agent.remainingDistance > tolerance) return;

        currentIndex++;
        if (currentIndex < path.Length)
            agent.SetDestination(path[currentIndex].position);
        else
        {
            float currentHealth = GetCurrentHealth();
            GameManager.Instance.playerBase.TakeDamage(currentHealth);
            Die();
        }
    }

    protected int GetClosestWaypointIndex()
    {
        int closestIndex = 0;
        float closestDist = float.MaxValue;

        Vector3 pos = transform.position;
        for (int i = 0; i < path.Length; i++)
        {
            float dist = Vector3.SqrMagnitude(path[i].position - pos);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    protected abstract void Update();// Let children override entirely
}

