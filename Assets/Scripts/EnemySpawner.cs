using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [Tooltip("SiegeEnemy = 0\nSkirmishEnemy = 1")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [SerializeField] private Transform spawnPoint;

    [Header("Spawning Settings")]
    [SerializeField] public int maxEnemies = 20;
    [SerializeField] private float spawnRateBetweenEnemies = 1f;
    [SerializeField] private float delayBetweenGroups = 5f;
    [SerializeField] private int enemiesInGroup;

    [Header("Siege Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float siegeSpawnChance = 0.3f; // 30% chance to include Siege enemies
    [SerializeField] private int maxSiegePerGroup = 1;

    private int enemiesLeft;
    private int groupsSpawned = 0;
    private bool spawningGroup = false;

    private void Start()
    {
        enemiesLeft = maxEnemies;
        StartCoroutine(SpawnGroupsLoop());
    }

    public int GetEnemiesLeft()
    {
        return enemiesLeft;
    }

    private IEnumerator SpawnGroupsLoop()
    {
        while (enemiesLeft > 0)
        {
            if (!GameManager.Instance.allowSpawning)
            {
                yield return null;
                continue;
            }

            if (!spawningGroup)
            {
                spawningGroup = true;
                yield return StartCoroutine(SpawnGroup());

                groupsSpawned++;
                if (groupsSpawned % 3 == 0)
                {
                    enemiesInGroup++;
                    Debug.Log($"Increased group size to {enemiesInGroup} after {groupsSpawned} groups.");
                }

                yield return new WaitForSeconds(delayBetweenGroups);
                spawningGroup = false;
            }

            yield return null;
        }
    }

    private IEnumerator SpawnGroup()
    {
        int groupSize = Mathf.Min(enemiesInGroup, enemiesLeft);

        int siegeCount = 0;

        if (Random.value < siegeSpawnChance)
        {
            siegeCount = Random.Range(1, maxSiegePerGroup + 1);
            siegeCount = Mathf.Min(siegeCount, groupSize);
        }

        int skirmishCount = groupSize - siegeCount;

        Debug.Log($"Spawning group: {siegeCount} Siege, {skirmishCount} Skirmish");

        for (int i = 0; i < siegeCount; i++)
        {
            SpawnEnemy(0); // Siege
            yield return new WaitForSeconds(spawnRateBetweenEnemies);
        }

        for (int i = 0; i < skirmishCount; i++)
        {
            SpawnEnemy(1); // Skirmish
            yield return new WaitForSeconds(spawnRateBetweenEnemies);
        }
    }

    public void SpawnEnemy(int index)
    {
        if (enemiesLeft <= 0) 
            return;

        if (index < 0 || index >= enemyPrefabs.Length)
        {
            Debug.LogWarning("Invalid enemy index: " + index);
            return;
        }

        Instantiate(enemyPrefabs[index], spawnPoint.position, spawnPoint.rotation);
        enemiesLeft--;

        UIManager.Instance.UpdateWaveBar(enemiesLeft, maxEnemies);
    }

    public void SetEnemiesPerGroup(int amount)
    {
        enemiesInGroup = amount;
    }

    public void SetSiegeSpawnChance(float chance)
    {
        siegeSpawnChance = Mathf.Clamp01(chance);
    }

    public void StartNextWave(int wave)
    {
        maxEnemies += 5;
        enemiesLeft = maxEnemies;
        enemiesInGroup = 3 + wave / 2;
        siegeSpawnChance = siegeSpawnChance + 0.05f;
        Debug.Log($"Wave {wave} spawned: maxEnemies={maxEnemies}, siegeChance={siegeSpawnChance}");
    }
}
