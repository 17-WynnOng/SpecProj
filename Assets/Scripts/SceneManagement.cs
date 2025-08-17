using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public static SceneManagement Instance;

    [Header("Game Scenes")]
    [Tooltip("List of playable scene names")]
    [SerializeField] private List<string> gameScenes = new List<string>();

    [SerializeField] private string winSceneName, loseSceneName, mainMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        Cursor.lockState = CursorLockMode.None;
    }

    public void LoadSceneAndSave(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        GameManager.Instance.SaveWinData();
        LoadoutManager.Instance.SaveUnlockedData();
    }

    public void LoadSceneClearData(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        Cursor.lockState = CursorLockMode.None;
        LoadoutManager.Instance.ClearUnlocksInMemory();
        LoadoutManager.Instance.DeleteUnlockedData();
    }

    public void LoadRandomScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (!currentSceneName.Equals(mainMenu, System.StringComparison.OrdinalIgnoreCase))
        {
            if (GameManager.Instance != null && GameManager.Instance.sectorsCleared >= GameManager.Instance.winObjective)
            {
                LoadSceneClearData(winSceneName);
                return;
            }
        }

        if (gameScenes.Count <= 1)
        {
            Debug.LogWarning("Not enough scenes in list to pick a different one.");
            return;
        }

        string chosenScene;
        do
        {
            chosenScene = gameScenes[Random.Range(0, gameScenes.Count)];
        }
        while (chosenScene == currentSceneName);

        LoadScene(chosenScene);
    }

    public string GetSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}
