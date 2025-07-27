using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;
using System.ComponentModel;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Game UI")]
    public RectTransform healthBar;
    public RectTransform baseHealthBar;
    public RectTransform waveBar;
    public TMP_Text magazineTxt;
    public TMP_Text reserveAmmoTxt;
    public TMP_Text gunTxt;
    public TMP_Text buildStatusTxt;
    public TMP_Text waveCountDownText;
    public TMP_Text waveText;
    public TMP_Text scrapAmtTxt;
    public GameObject gameUICanvas;
    public GameObject middleLeftUI;
    public TMP_Text buildCostText;

    [Header("Loadout UI")]
    public GameObject loadoutUICanvas;
    public TMP_Text selectedPrimaryTxt;
    public TMP_Text selectedSecondaryTxt;
    public TMP_Text[] selectedSentriesTxt;
    public RawImage scanLines;

    private float healthBarMaxWidth;
    private float baseHPBarMaxWidth;
    private float waveBarMaxWidth;

    private void Awake()
    {
        middleLeftUI.SetActive(false);
        gameUICanvas.SetActive(false);
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
        waveBarMaxWidth = waveBar.sizeDelta.x;
    }

    public void UpdateSentryList(DeployableData[] deployables)
    {
        for (int i = 0; i < selectedSentriesTxt.Length; i++)
        {
            if (i < deployables.Length && deployables[i] != null)
                selectedSentriesTxt[i].text = deployables[i].deployableName;
            else
                selectedSentriesTxt[i].text = "—";    // or blank
        }
    }

    public void UpdateAmmoUI(int currentMag, int currentReserve)
    {
        magazineTxt.text = currentMag.ToString();
        reserveAmmoTxt.text = currentReserve.ToString();
    }

    public void UpdatePlayerHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar == null)
            return;

        float percent = Mathf.Clamp01(currentHealth / maxHealth);
        Vector2 size = healthBar.sizeDelta;
        size.x = percent * healthBarMaxWidth;
        healthBar.sizeDelta = size;
    }

    public void UpdateBaseHealthBar(float baseCurrentHealth, float baseMaxHealth)
    {
        if (baseHealthBar == null)
            return;

        float percent = Mathf.Clamp01(baseCurrentHealth / baseMaxHealth);
        Vector2 size = baseHealthBar.sizeDelta;
        size.x = percent * baseHPBarMaxWidth;
        baseHealthBar.sizeDelta = size;
    }

    public void UpdateWaveBar(int enemiesLeft, int maxEnemies)
    {
        if (waveBar == null)
            return;

        float percent = Mathf.Clamp01((float)enemiesLeft / maxEnemies);
        Vector2 size = waveBar.sizeDelta;
        size.x = percent * waveBarMaxWidth;
        waveBar.sizeDelta = size;
    }

    public void UpdateWaveCount(int currentWave, int winWave)
    {
        waveText.text = currentWave.ToString() + "/" + winWave.ToString();
    }

    public void UpdateScrapCount(int currentScrap)
    {
        scrapAmtTxt.text = currentScrap.ToString();
    }

    public void UpdateDeployableCost(int currentScrap, int deployableCost)
    {
        buildCostText.text = currentScrap.ToString() + "/" + deployableCost.ToString();
    }

    public void InitializeBuildToolUI(GameObject buildToolInstance)
    {
        Transform txtTransform = buildToolInstance.transform.Find("Canvas/BuildCost_Txt");

        if (txtTransform != null)
        {
            TMP_Text costText = txtTransform.GetComponent<TMP_Text>();
            if (costText != null)
            {
                buildCostText = costText;
            }
            else
            {
                Debug.LogWarning("BuildCost_Txt found, but TextMeshProUGUI component is missing.");
            }
        }
        else
        {
            Debug.LogWarning("BuildCost_Txt not found in BuildTool prefab.");
        }
    }
}
