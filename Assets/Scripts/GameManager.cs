using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public BaseHealth playerBase;
    public LevelPath levelPath;
    public bool allowSpawning = false;

    [SerializeField] private float waveCountdownDuration = 60f;
    private float countdownRemaining;
    private bool countdownActive = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
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
        UIManager.Instance.middleLeftUI.SetActive(false);
    }
}
