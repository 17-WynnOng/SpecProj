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
        LoadoutManager.Instance.SetLoadout(selectedPrimary, selectedSecondary);

        // Find the player's WeaponInventory
        PlayerLoadout weaponInventory = FindObjectOfType<PlayerLoadout>();
        if (weaponInventory != null)
        {
            weaponInventory.LoadPlayerLoadout();
        }

        // Optionally hide UI
        gameObject.SetActive(false);
    }
}
