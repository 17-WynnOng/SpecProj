using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance;
    [SerializeField] private GameObject continueBtn;
    [SerializeField] private Transform mainMenuBtnGroup;
    [SerializeField] private Transform startingLoadoutGroup;
    [SerializeField] private GameObject startingLoadoutUI;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (SaveSystem.Exists("unlocks.json"))
        {
            GameObject instance = Instantiate(continueBtn, mainMenuBtnGroup);
            Button button = instance.GetComponent<Button>();

            button.onClick.AddListener(() =>
            {
                // Example: Load saved unlocks and go to the next scene
                LoadoutManager.Instance.LoadUnlockedData();
                ContinueScene();
            });
        }
    }

    public void ContinueScene()
    {
        if (!SaveSystem.Exists("gameStage.json"))
        {
            Debug.Log("No gameStage save found.");
            return;
        }

        WinSaveData data = SaveSystem.Load<WinSaveData>("gameStage.json");
        string name = data.levelName;
        SceneManagement.Instance.LoadScene(name);
    }

    public void ViewPanel(GameObject open)
    {
        open.SetActive(true);
    }   

    public void ClosePanel(GameObject gameobject)
    {
        gameobject.SetActive(false);
    }

    public void StartNewGame()
    {
        foreach (Transform child in startingLoadoutGroup)
        {
            Destroy(child.gameObject);
        }

        SaveSystem.Delete("gameStage.json");

        LoadoutManager.Instance.DeleteUnlockedData();
        LoadoutManager.Instance.ClearUnlocksInMemory();

        LoadoutManager.Instance.UnlockWeapon("assault_rifle_01");
        LoadoutManager.Instance.UnlockDeployable("mg_sentry");

        LoadoutManager.Instance.LoadUnlockedData();

        foreach (var weapon in LoadoutManager.Instance.unlockedWeapons)
        {
           
            GameObject btn = Instantiate(startingLoadoutUI, startingLoadoutGroup);
            btn.GetComponent<StartingLoadout_UI>().InitializeWeapon(weapon);
        }

        foreach (var deployable in LoadoutManager.Instance.unlockedDeployables)
        {
            GameObject btn = Instantiate(startingLoadoutUI, startingLoadoutGroup);
            btn.GetComponent<StartingLoadout_UI>().InitializeDeployable(deployable);
        }
    }
}
