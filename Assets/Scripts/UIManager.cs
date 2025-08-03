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

    [Header("Bottom Left")]
    public RectTransform healthBar;
    public TMP_Text scrapAmtTxt;

    [Header("Bottom Right")]
    public TMP_Text magazineTxt;
    public TMP_Text reserveAmmoTxt;
    public TMP_Text gunTxt;

    [Header("Top Right")]
    public RectTransform baseHealthBar;
    public TMP_Text waveText;

    [Header("Top Left")]
    public GameObject topLeftUI;
    public GameObject[] spawnerUI;
    public TMP_Text[] spawnerNames;
    public RectTransform[] waveBars;

    [Header("Middle Left")]
    public GameObject middleLeftUI;
    public TMP_Text waveCountDownText;

    [Header("Build Tool")]
    public TMP_Text heldDeployable;
    private TMP_Text buildCostTxt, recycleRefundTxt;
    private GameObject buildModeUI, sellModeUI;

    [Header("Enemy Spawner Txt")]
    public TMP_Text[] enemySpawnerName;

    [Header("Canvas")]
    public GameObject gameUICanvas;

    [Header("Loadout UI")]
    public GameObject loadoutUICanvas;
    public TMP_Text selectedPrimaryTxt;
    public TMP_Text selectedSecondaryTxt;
    public TMP_Text[] selectedSentriesTxt;

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

        foreach (RectTransform transform in waveBars)
        {
            waveBarMaxWidth = transform.sizeDelta.x;
        }
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
    public void UpdateSpawnerNames(List<EnemySpawner> activeSpawners)
    {

        // Clear all UI slots first
        for (int i = 0; i < spawnerNames.Length; i++)
        {
            spawnerNames[i].text = "";
        }

        // Assign active spawner names to the first few slots
        for (int i = 0; i < activeSpawners.Count && i < spawnerNames.Length; i++)
        {
            spawnerNames[i].text = activeSpawners[i].spawnerName;
        }
    }

    public void SpawnerUIState(List<EnemySpawner> activeSpawners)
    {
        // Set UI as false first
        for (int i = 0; i < spawnerUI.Length; i++)
        {
            spawnerUI[i].SetActive(false);
        }

        for (int i = 0; i < activeSpawners.Count && i < spawnerUI.Length; i++)
        {
            spawnerUI[i].SetActive(true);
        }
    }

    public void UpdateWaveBars(List<EnemySpawner> activeSpawners)
    {
        // First hide all bars
        for (int i = 0; i < waveBars.Length; i++)
        {
            waveBars[i].gameObject.SetActive(false);
        }

        // Then activate and update only the ones for active spawners
        for (int i = 0; i < activeSpawners.Count && i < waveBars.Length; i++)
        {
            waveBars[i].gameObject.SetActive(true);

            int enemiesLeft = activeSpawners[i].GetEnemiesLeft();
            int maxEnemies = activeSpawners[i].maxEnemies;

            float percent = maxEnemies > 0 ? Mathf.Clamp01((float)enemiesLeft / maxEnemies) : 0f;
            Vector2 size = waveBars[i].sizeDelta;
            size.x = percent * waveBarMaxWidth;
            waveBars[i].sizeDelta = size;
        }
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
        buildCostTxt.text = currentScrap.ToString() + "/" + deployableCost.ToString();
    }

    public void UpdateRecycleCost(int deployableCost)
    {
        recycleRefundTxt.text = deployableCost.ToString();
    }
        
    public void UpdateHeldDeployable(string name)
    {
        heldDeployable.text = name;
    }

    public void EnableBuildUI()
    {
        buildModeUI.SetActive(true);
        sellModeUI.SetActive(false);
    }

    public void EnableSellUI()
    {
        buildModeUI.SetActive(false);
        sellModeUI.SetActive(true);
    }

    public void DisableBuildToolUI()
    {
        buildModeUI.SetActive(false);
        sellModeUI.SetActive(false);
    }

    public void UpdateEnemySpawnerNames(EnemySpawner[] spawners)
    {
        for (int i = 0; i < enemySpawnerName.Length && i < spawners.Length; i++)
        {
            if (enemySpawnerName[i] != null && spawners[i] != null)
            {
                enemySpawnerName[i].text = spawners[i].spawnerName;
            }
        }
    }

    public void InitializeBuildToolUI(GameObject buildToolInstance)
    {
        Transform root = buildToolInstance.transform;

        // Initialize BuildMode_UI
        Transform buildModeTransform = root.Find("Canvas/BuildMode_UI");
        if (buildModeTransform != null)
        {
            buildModeUI = buildModeTransform.gameObject;

            Transform costTxtTransform = buildModeTransform.Find("BuildCost_Txt");
            if (costTxtTransform != null)
            {
                buildCostTxt = costTxtTransform.GetComponent<TMP_Text>();
                if (buildCostTxt == null)
                    Debug.LogWarning("BuildCost_Txt found, but TMP_Text component is missing.");
            }
            else
            {
                Debug.LogWarning("BuildCost_Txt not found under BuildMode_UI.");
            }
        }
        else
        {
            Debug.LogWarning("BuildMode_UI not found.");
        }

        // Initialize SellMode_UI
        Transform sellModeTransform = root.Find("Canvas/SellMode_UI");
        if (sellModeTransform != null)
        {
            sellModeUI = sellModeTransform.gameObject;

            Transform refundTxtTransform = sellModeTransform.Find("RecycleRefund_Txt");
            if (refundTxtTransform != null)
            {
                recycleRefundTxt = refundTxtTransform.GetComponent<TMP_Text>();
                if (recycleRefundTxt == null)
                    Debug.LogWarning("RecycleRefund_Txt found, but TMP_Text component is missing.");
            }
            else
            {
                Debug.LogWarning("RecycleRefund_Txt not found under SellMode_UI.");
            }
        }
        else
        {
            Debug.LogWarning("SellMode_UI not found.");
        }
    }
}
