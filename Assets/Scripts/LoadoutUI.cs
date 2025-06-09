using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutUI : MonoBehaviour
{
    public List<WeaponData> allWeapons; // assign in inspector
    public WeaponData selectedPrimary;
    public WeaponData selectedSecondary;

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

    public void ConfirmLoadout()
    {
        if (selectedPrimary != null && selectedSecondary != null)
        {
            LoadoutManager.Instance.SetLoadout(selectedPrimary, selectedSecondary);
                
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
