using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockCardSpawner : MonoBehaviour
{
    [SerializeField] private GameObject unlockCardPrefab;
    [SerializeField] private Transform cardHolder;
    [SerializeField] private int numberOfCards = 3;
    [SerializeField] private GameObject continueBtn;

    private List<UnlockCard> spawnedCards = new List<UnlockCard>();

    private void Start()
    {
        if (SceneManagement.Instance != null)
        {
            if (continueBtn == null) 
                return;

            var btn = continueBtn.GetComponent<Button>();
            if (btn == null) 
                return;

            btn.onClick.RemoveAllListeners();

            if (SceneManagement.Instance != null)
            {
                btn.onClick.AddListener(SceneManagement.Instance.LoadRandomScene);
                btn.onClick.AddListener(LoadoutManager.Instance.SaveUnlockedData);

                if (GameManager.Instance != null)
                {
                    btn.onClick.AddListener(GameManager.Instance.AddSectorsCleared);
                    btn.onClick.AddListener(GameManager.Instance.SaveWinData);
                }
            }
        }
    }

    public void SpawnUnlockCards()
    {
        ClearExistingCards();
        spawnedCards.Clear();
        continueBtn.SetActive(false);

        //store unique hashset, prevents duplicates
        var unlockedWeaponIDs = new HashSet<string>(
            LoadoutManager.Instance.unlockedWeapons.ConvertAll(w => w.weaponID));

        var unlockedDeployableIDs = new HashSet<string>(
            LoadoutManager.Instance.unlockedDeployables.ConvertAll(d => d.deployableID));

        // Filter by ID (not reference)
        var lockedWeapons = LoadoutManager.Instance.equipmentDB.allWeapons
            .FindAll(w => !unlockedWeaponIDs.Contains(w.weaponID));

        var lockedDeployables = LoadoutManager.Instance.equipmentDB.allDeployables
            .FindAll(d => !unlockedDeployableIDs.Contains(d.deployableID));

        int totalLocked = lockedWeapons.Count + lockedDeployables.Count;
        int spawnCount = Mathf.Min(numberOfCards, totalLocked);

        for (int i = 0; i < spawnCount; i++)
        {
            var cardObj = Instantiate(unlockCardPrefab, cardHolder);
            var card = cardObj.GetComponent<UnlockCard>();
            card.InitializeSpawner(this);
            card.AssignContinueBtn(continueBtn);
            spawnedCards.Add(card);

            bool canPickWeapon = lockedWeapons.Count > 0;
            bool canPickDeploy = lockedDeployables.Count > 0;

            // pick from whichever list still has items
            bool pickWeapon = (canPickWeapon && (!canPickDeploy || Random.value > 0.5f));

            if (pickWeapon)
            {
                int idx = Random.Range(0, lockedWeapons.Count);
                var w = lockedWeapons[idx];
                lockedWeapons.RemoveAt(idx);
                card.AssignWeapon(w);
            }
            else
            {
                int idx = Random.Range(0, lockedDeployables.Count);
                var d = lockedDeployables[idx];
                lockedDeployables.RemoveAt(idx);
                card.AssignDeployable(d);
            }
        }

        // If nothing left to unlock, show continue immediately
        continueBtn.SetActive(totalLocked == 0);
    }

    public void DisableAllCards(UnlockCard selected)
    {
        foreach (var card in spawnedCards)
        {
            card.DisableButton();
        }
    }

    private void ClearExistingCards()
    {
        foreach (Transform child in cardHolder)
        {
            Destroy(child.gameObject);
        }
    }
}
