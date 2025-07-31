using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance;
    [SerializeField] private GameObject continueBtn;
    [SerializeField] private Transform content;

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
            GameObject instance = Instantiate(continueBtn, content);
            Button button = instance.GetComponent<Button>();

            button.onClick.AddListener(() =>
            {
                // Example: Load saved unlocks and go to the next scene
                LoadoutManager.Instance.LoadUnlockedData();
                SceneManagement.Instance.LoadScene("Level1Scene");
            });
        }
    }

    public void ViewPanel(GameObject open)
    {
        open.SetActive(true);
    }   

    public void ClosePanel(GameObject gameobject)
    {
        gameobject.SetActive(false);
    }
}
