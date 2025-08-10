using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Spawner Name")]
    public string spawnerName;

    [Header("Enemy Settings")]
    [Tooltip("SiegeEnemy = 0\nSkirmishEnemy = 1")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [SerializeField] private Transform spawnPoint;

    [Header("Spawning Settings")]
    public int maxEnemies = 20;
    [SerializeField] private float spawnRateBetweenEnemies = 1f;
    [SerializeField] private int groupSpawnDelayMin, groupSpawnDelayMax;
    [SerializeField] private int enemiesInGroup;
    [SerializeField] private LevelPath levelPath;

    [Header("Siege Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float siegeSpawnChance = 0.3f; // 30% chance to include Siege enemies
    [SerializeField] private int maxSiegePerGroup = 1;

    public int enemiesLeft;
    [SerializeField] private int groupsSpawned = 0;
    private bool spawningGroup = false;

    private Coroutine spawnLoop;

    private int debugSpawnedTotal = 0;

    private void Start()
    {
        enemiesLeft = maxEnemies;
        //spawnLoop = StartCoroutine(SpawnGroupsLoop());

        UIManager.Instance.UpdateEnemySpawnerNames(GameManager.Instance.enemySpawners);
    }

    public int GetEnemiesLeft()
    {
        return enemiesLeft;
    }

    public void StopSpawning()
    {
        if (spawnLoop != null)
        {
            StopCoroutine(spawnLoop);
            spawnLoop = null;
            spawningGroup = false;
        }
    }

    private IEnumerator SpawnGroupsLoop()
    {
        yield return new WaitForSeconds(Random.Range(0f, 3f));

        while (enemiesLeft > 0)
        {
            if (!GameManager.Instance.allowSpawning)
            {
                Debug.LogWarning("Spawning was disabled, exiting SpawnGroupsLoop.");
                yield break; // stop the coroutine entirely
            }

            if (enemiesLeft <= 0)
                yield break;

            if (!spawningGroup)
            {
                if (!GameManager.Instance.allowSpawning)
                {
                    Debug.LogWarning("Spawning was disabled, exiting SpawnGroupsLoop.");
                    yield break; // stop the coroutine entirely
                }

                if (enemiesLeft <= 0)
                    yield break;

                spawningGroup = true;
                yield return StartCoroutine(SpawnGroup());

                groupsSpawned++;
                if (groupsSpawned % 4 == 0)
                {
                    enemiesInGroup++;
                    Debug.Log($"Increased group size to {enemiesInGroup} after {groupsSpawned} groups.");
                }

                yield return new WaitForSeconds(Random.Range(groupSpawnDelayMin, groupSpawnDelayMax + 1));
                spawningGroup = false;
            }

            yield return null;
        }
    }

    private IEnumerator SpawnGroup()
    {
        if (enemiesLeft <= 0)
            yield break;

        if (!GameManager.Instance.allowSpawning)
        {
            Debug.LogWarning("Tried to spawn while spawning is disabled.");
            yield break;
        }

        int groupSize = Mathf.Min(enemiesInGroup, enemiesLeft);

        int siegeCount = 0;

        if (Random.value < siegeSpawnChance)
        {
            siegeCount = Random.Range(1, maxSiegePerGroup + 1);
        }

        siegeCount = Mathf.Min(siegeCount, groupSize);
        int skirmishCount = groupSize - siegeCount;

        Debug.Log($"Spawning group: {siegeCount} Siege, {skirmishCount} Skirmish");

        int totalToSpawn = siegeCount + skirmishCount;
        int siegeSpawned = 0;
        int skirmishSpawned = 0;

        for (int i = 0; i < totalToSpawn; i++)
        {
            if (!GameManager.Instance.allowSpawning || enemiesLeft <= 0)
                break;

            if (siegeSpawned < siegeCount)
            {
                SpawnEnemy(0);
                siegeSpawned++;
            }
            else if (skirmishSpawned < skirmishCount)
            {
                SpawnEnemy(1);
                skirmishSpawned++;
            }

            yield return new WaitForSeconds(spawnRateBetweenEnemies);
        }
    }

    public void SpawnEnemy(int index)
    {
        if (enemiesLeft <= 0)
        {
            Debug.LogWarning("Tried to spawn enemy when none left!");
            return;
        }

        if (index < 0 || index >= enemyPrefabs.Length)
        {
            Debug.LogWarning("Invalid enemy index: " + index);
            return;
        }

        GameObject enemy = Instantiate(enemyPrefabs[index], spawnPoint.position, spawnPoint.rotation);

        enemiesLeft--;
        debugSpawnedTotal++;
        Debug.Log($"Spawned: {debugSpawnedTotal} / {maxEnemies} | enemiesLeft: {enemiesLeft}");

        UIManager.Instance.UpdateWaveBars(GameManager.Instance.activeSpawners);

        // Check for EnemyAI and assign path
        if (enemy.TryGetComponent<EnemyAI>(out var ai))
        {
            ai.path = levelPath.waypoints;
        }

        //Add to GameManager's list
        GameManager.Instance.aliveEnemies.Add(enemy);

        //remove from list when enemy dies
        if (enemy.TryGetComponent<EnemyAI>(out var dmg))
        {
            dmg.onDeath += () =>
            {
                GameManager.Instance.aliveEnemies.Remove(enemy);
                GameManager.Instance.CheckWaveEnd();
            };
        }
    }

    public void BeginSpawning()
    {
        if (spawnLoop != null)
            StopCoroutine(spawnLoop);

        if (!GameManager.Instance.allowSpawning)
        {
            Debug.LogWarning("Tried to start spawning when disallowed.");
            return;
        }

        spawnLoop = StartCoroutine(SpawnGroupsLoop());
    }

    public void StartNextWave(int wave)
    {
        enemiesInGroup = 3 + wave / 2;
        siegeSpawnChance = Mathf.Min(0.15f + 0.05f, 0.5f);    

        groupsSpawned = 0;          // reset group count for wave pacing
        spawningGroup = false;      // reset group state

        Debug.Log($"Wave {wave} spawned: maxEnemies={maxEnemies}, siegeChance={siegeSpawnChance}");

        enemiesLeft = maxEnemies;

        if (spawnLoop != null)
            StopCoroutine(spawnLoop);
    }
}
