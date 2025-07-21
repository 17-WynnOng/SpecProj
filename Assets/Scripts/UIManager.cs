using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Game UI")]
    public GameObject gameUICanvas;
    public RectTransform healthBar;
    public RectTransform baseHealthBar;
    public TMP_Text magazineTxt;
    public TMP_Text reserveAmmoTxt;
    public TMP_Text gunTxt;
    public TMP_Text buildStatusTxt;
    public TMP_Text waveCountDownText;
    public GameObject middleLeftUI;

    [Header("Loadout UI")]
    public GameObject loadoutUICanvas;
    public TMP_Text selectedPrimaryTxt;
    public TMP_Text selectedSecondaryTxt;
    public TMP_Text[] selectedSentriesTxt;

    private float healthBarMaxWidth;
    private float baseHPBarMaxWidth;

    private void Awake()
    {
        middleLeftUI.SetActive(false);
        gameUICanvas.SetActive(true);
        loadoutUICanvas.SetActive(true);

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        healthBarMaxWidth = healthBar.sizeDelta.x;
        baseHPBarMaxWidth = baseHealthBar.sizeDelta.x;
    }

    public void UpdateSentryList(WeaponData[] sentries)
    {
        for (int i = 0; i < selectedSentriesTxt.Length; i++)
        {
            if (i < sentries.Length && sentries[i] != null)
                selectedSentriesTxt[i].text = sentries[i].weaponName;
            else
                selectedSentriesTxt[i].text = "—";    // or blank
        }
    }

    public float GetHPMaxWidth()
    {
        return healthBarMaxWidth;
    }

    public float GetBaseHPMaxWidth()
    {
        return baseHPBarMaxWidth;
    }
}
