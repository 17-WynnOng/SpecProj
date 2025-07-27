using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
