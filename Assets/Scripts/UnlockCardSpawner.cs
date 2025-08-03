using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockCardSpawner : MonoBehaviour
{
    [SerializeField] private GameObject unlockCardPrefab;
    [SerializeField] private Transform cardHolder;
    [SerializeField] private int numberOfCards = 3;
    [SerializeField] private GameObject continueBtn;

    private List<UnlockCard> spawnedCards = new List<UnlockCard>();

    public void SpawnUnlockCards()
    {
        ClearExistingCards();
        spawnedCards.Clear();
        continueBtn.SetActive(false);

        var lockedWeapons = LoadoutManager.Instance.equipmentDB.allWeapons.FindAll(w => !LoadoutManager.Instance.unlockedWeapons.Contains(w));
        var lockedDeployables = LoadoutManager.Instance.equipmentDB.allDeployables.FindAll(d => !LoadoutManager.Instance.unlockedDeployables.Contains(d));

        int totalLocked = lockedWeapons.Count + lockedDeployables.Count;
        int spawnCount = Mathf.Min(numberOfCards, totalLocked);

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject cardObj = Instantiate(unlockCardPrefab, cardHolder);
            UnlockCard card = cardObj.GetComponent<UnlockCard>();
            card.InitializeSpawner(this);
            spawnedCards.Add(card);
            card.AssignContinueBtn(continueBtn);

            bool pickWeapon = Random.value > 0.5f;

            if (pickWeapon && lockedWeapons.Count > 0)
            {
                WeaponData w = lockedWeapons[Random.Range(0, lockedWeapons.Count)];
                lockedWeapons.Remove(w); // avoid duplicates
                card.AssignWeapon(w);
            }
            else if (lockedDeployables.Count > 0)
            {
                DeployableData d = lockedDeployables[Random.Range(0, lockedDeployables.Count)];
                lockedDeployables.Remove(d); // avoid duplicates
                card.AssignDeployable(d);
            }
            else
            {
                card.gameObject.SetActive(false); // fallback: hide empty card
            }
        }
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
