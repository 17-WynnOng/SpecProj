using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Spawning Settings")]
    [SerializeField] private float spawnRate = 2f; //Seconds
    [SerializeField] private int maxEnemies = 10;  // Optional: limit number of enemies
    private float spawnTimer;
    private int enemiesSpawned = 0;

    private void Update()
    {
        if (!GameManager.Instance.allowSpawning)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnRate && enemiesSpawned < maxEnemies)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemiesSpawned++;
    }
}
