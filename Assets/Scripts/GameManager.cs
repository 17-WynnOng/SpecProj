using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public BaseHealth playerBase;
    public EnemySpawner[] enemySpawners = new EnemySpawner[4];
    public bool allowSpawning = false;

    [SerializeField] private float waveCountdownDuration = 60f;
    private float countdownRemaining;
    private bool countdownActive = false;

    public int winWave = 4;
    public int currentWave = 1;

    public List<GameObject> aliveEnemies = new List<GameObject>();

    [HideInInspector ]public List<EnemySpawner> activeSpawners = new List<EnemySpawner>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UIManager.Instance.UpdateWaveCount(currentWave, winWave);
        allowSpawning = false;
        SelectSpawners();
    }

    private void Update()
    {
        CheckWaveEnd();

        if (!countdownActive) return;

        countdownRemaining -= Time.deltaTime;

        int minutes = Mathf.FloorToInt(countdownRemaining / 60f);
        int seconds = Mathf.FloorToInt(countdownRemaining % 60f);
        UIManager.Instance.waveCountDownText.text = $"{minutes}:{seconds:00}";

        if (countdownRemaining <= 0f)
        {
            EndCountdown();
        }
    }

    public void CheckWaveEnd()
    {
        foreach (EnemySpawner spawner in enemySpawners)
        {
            if (spawner.isActiveAndEnabled)
            {
                if (aliveEnemies.Count == 0 && GetTotalEnemiesLeft() <= 0)
                {
                    allowSpawning = false;
                    TryAdvanceWave(); // or Win if last wave
                }
            }
        }
    }

    public void StartWaveCountdown()
    {
        countdownRemaining = waveCountdownDuration;
        countdownActive = true;
        allowSpawning = false;
        UIManager.Instance.middleLeftUI.SetActive(true);
    }

    public void EndCountdown()
    {
        countdownActive = false;
        allowSpawning = true;

        foreach (EnemySpawner spawner in enemySpawners)
        {
            spawner.enemiesLeft = spawner.maxEnemies;
            UIManager.Instance.middleLeftUI.SetActive(false);
            UIManager.Instance.UpdateWaveBars(activeSpawners);
        }

        BeginSpawning();
    }

    private void SelectSpawners()
    {
        // Disable all spawners
        foreach (var spawner in enemySpawners)
        {
            spawner.StopSpawning();
            spawner.gameObject.SetActive(false);
        }

        activeSpawners.Clear();

        int spawnerCount = enemySpawners.Length;
        int toEnable = currentWave >= 3 ? 2 : 1;
        toEnable = Mathf.Min(toEnable, spawnerCount);

        HashSet<int> pickedIndices = new HashSet<int>();
        while (pickedIndices.Count < toEnable)
        {
            int index = Random.Range(0, spawnerCount);
            pickedIndices.Add(index); //Hashset checks if index is already added, if it already exists it doesn't add
        }

        foreach (int i in pickedIndices)
        {
            var spawner = enemySpawners[i];
            spawner.gameObject.SetActive(true);
            activeSpawners.Add(spawner);
        }

        UIManager.Instance.UpdateSpawnerNames(activeSpawners);
        UIManager.Instance.SpawnerUIState(activeSpawners);
    }

    private void BeginSpawning()
    {
        foreach (var spawner in activeSpawners)
        {
            spawner.enemiesLeft = spawner.maxEnemies;
            spawner.StartNextWave(currentWave);
            spawner.BeginSpawning();
            UIManager.Instance.UpdateWaveBars(activeSpawners);
        }
    }

    public void TryAdvanceWave()
    {
        bool allEnemiesDead = aliveEnemies.Count == 0;
        bool noEnemiesLeftToSpawn = GetTotalEnemiesLeft() <= 0;

        if (!noEnemiesLeftToSpawn || !allEnemiesDead)
            return;

        allowSpawning = false;

        // Stop all active spawners
        foreach (EnemySpawner spawner in enemySpawners)
        {
            if (spawner.isActiveAndEnabled)
                spawner.StopSpawning();
        }

        // Handle wave progression
        if (currentWave < winWave)
        {
            currentWave++;
            StartWaveCountdown();
            SelectSpawners();
            UIManager.Instance.UpdateWaveCount(currentWave, winWave);
        }
        else if (currentWave == winWave)
        {
            SceneManagement.Instance.LoadScene("WinScene");
        }
    }

    public int GetTotalEnemiesLeft()
    {
        int total = 0;
        foreach (var spawner in activeSpawners)
        {
            if (spawner.gameObject.activeInHierarchy)
                total += spawner.GetEnemiesLeft();
        }
        return total;
    }
}
