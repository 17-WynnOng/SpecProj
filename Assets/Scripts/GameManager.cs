using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public BaseHealth playerBase;
    public EnemySpawner enemySpawner;
    public LevelPath levelPath;
    public bool allowSpawning = false;

    [SerializeField] private float waveCountdownDuration = 60f;
    private float countdownRemaining;
    private bool countdownActive = false;

    public int winWave = 4;
    public int currentWave = 1;
    
    public int enemyCounter = 0;

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

    }

    private void Update()
    {
        if (allowSpawning && enemySpawner.GetEnemiesLeft() <= 0 && enemyCounter >= enemySpawner.maxEnemies)
        {
            HandleWaveEnd();
        }

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

    private void HandleWaveEnd()
    {
        allowSpawning = false; // Immediately stop spawner
        if (enemySpawner != null)
            enemySpawner.StopSpawning();

        TryAdvanceWave(); // Existing logic
    }

    public void StartWaveCountdown()
    {
        enemyCounter = 0;
        countdownRemaining = waveCountdownDuration;
        countdownActive = true;
        allowSpawning = false;
        UIManager.Instance.middleLeftUI.SetActive(true);
    }

    public void EndCountdown()
    {
        countdownActive = false;
        allowSpawning = true;
        enemySpawner.enemiesLeft = enemySpawner.maxEnemies;
        UIManager.Instance.middleLeftUI.SetActive(false);
        UIManager.Instance.UpdateWaveBar(enemySpawner.GetEnemiesLeft(), enemySpawner.maxEnemies);

        if (enemySpawner != null)
        {
            enemySpawner.BeginSpawning();
        }
    }
    public void TryAdvanceWave()
    {
        if (!allowSpawning)
            return;

        bool noEnemiesLeftToSpawn = enemySpawner.GetEnemiesLeft() <= 0;
        bool allEnemiesDead = enemyCounter >= enemySpawner.maxEnemies;

        if (noEnemiesLeftToSpawn && allEnemiesDead)
        {
            allowSpawning = false;
            enemySpawner.StopSpawning();

            if (currentWave < winWave)
            {
                currentWave++;
                enemyCounter = 0;
                enemySpawner.StartNextWave(currentWave);
                StartWaveCountdown();
                UIManager.Instance.UpdateWaveCount(currentWave, winWave);
            }
            else if (currentWave == winWave)
            {
                // Last wave is over, all enemies are dead → Win
                SceneManagement.Instance.LoadScene("WinScene");
            }
        }
    }
}
