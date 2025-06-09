using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutUI : MonoBehaviour
{
    public List<WeaponData> allWeapons; // assign in inspector
    public WeaponData selectedPrimary;
    public WeaponData selectedSecondary;
    public WeaponData[] selectedSentries;
    private void Awake()
    {
        // if someone messed with it in the inspector, or it's null,
        // re-create the array at length 4
        if (selectedSentries == null || selectedSentries.Length != 4)
            selectedSentries = new WeaponData[4];
    }
    public void SelectPrimary(WeaponData weapon)
    {
        selectedPrimary = weapon;
        Debug.Log("Primary Selected: " + weapon.weaponName);
    }

    public void SelectSecondary(WeaponData weapon)
    {
        selectedSecondary = weapon;
        Debug.Log("Secondary Selected: " + weapon.weaponName);
    }

    public void SelectSentries(WeaponData weapon)
    {
        // 1) Fill any empty slot
        for (int i = 0; i < selectedSentries.Length; i++)
        {
            if (selectedSentries[i] == null)
            {
                selectedSentries[i] = weapon;
                Debug.Log($"Added to slot {i}: {weapon.weaponName}");
                return;
            }
        }

        // shift everything right (drop the old last)
        for (int i = selectedSentries.Length - 1; i > 0; i--)
            selectedSentries[i] = selectedSentries[i - 1];

        // now put the new weapon in the first slot
        selectedSentries[0] = weapon;
        Debug.Log($"Slots full → pushed down and placed new in slot 0: {weapon.weaponName}");
    }

    public void ConfirmLoadout()
    {
        if (selectedPrimary != null && selectedSecondary != null)
        {
            LoadoutManager.Instance.SetLoadout(selectedPrimary, selectedSecondary, selectedSentries);
                
            // Find the player's WeaponInventory
            PlayerLoadout weaponInventory = FindObjectOfType<PlayerLoadout>();
            PlayerController playerController = FindObjectOfType<PlayerController>();

            if (weaponInventory != null)
            {
                weaponInventory.LoadPlayerLoadout();
            }

            if (playerController != null)
            {
                playerController.EnableControls(true);
            }

            Cursor.lockState = CursorLockMode.Locked;

            // Optionally hide UI
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Unable to deploy without full loadout");
        }
    }
}
