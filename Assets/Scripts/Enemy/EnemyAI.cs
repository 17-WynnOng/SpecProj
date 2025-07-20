using UnityEngine.AI;
using UnityEngine;

public enum EnemyState
{
    Patrol,
    Chase,
    Search,
    Attack
}

public abstract class EnemyAI : MonoBehaviour
{
    protected NavMeshAgent agent;
    [SerializeField] protected Transform[] path;
    [SerializeField] protected Transform player;
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

    protected virtual void Start()
    {
        path = GameManager.Instance.levelPath.waypoints;
        currentIndex = 0;
        if (path.Length > 0)
            agent.SetDestination(path[0].position);
    }

    protected abstract void Update();// Let children override entirely
}

