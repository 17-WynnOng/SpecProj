using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLoadout : MonoBehaviour
{
    [Header("Weapon Slots")]
    [SerializeField] private Transform weaponHolder;

    [Header("Held Weapon")]
    public Weapon heldWeapon;

    [SerializeField] private Weapon[] equippedWeapons = new Weapon[2];
    [HideInInspector] public int currentWeaponIndex = 0;

    [Header("Held Sentry")]
    public WeaponData heldSentry;

    [Tooltip("Prefabs for the two sentry types")]
    [SerializeField] public WeaponData[] equippedSentries;
    [HideInInspector] public int currentSentryIndex = 0; // which sentry prefab is selected

    public void LoadPlayerLoadout()
    {
        WeaponData primary = LoadoutManager.Instance.PrimaryWeapon;
        WeaponData secondary = LoadoutManager.Instance.SecondaryWeapon;
        equippedSentries = LoadoutManager.Instance.Sentries;

        EquipWeapon(0, primary);
        EquipWeapon(1, secondary);

        SwitchToWeapon(0); // Start with primary
        SwitchToSentry(0);
    }

    private void EquipWeapon(int slot, WeaponData data)
    {
        Weapon weapon = Instantiate(data.weaponPrefab, weaponHolder).GetComponent<Weapon>();
        weapon.Initialize(data, Camera.main);
        equippedWeapons[slot] = weapon;
    }

    public void SwitchToWeapon(int index)
    {
        for (int i = 0; i < equippedWeapons.Length; i++)
            equippedWeapons[i]?.gameObject.SetActive(i == index);

        currentWeaponIndex = index;
        heldWeapon = equippedWeapons[currentWeaponIndex];
        UIManager.Instance.gunTxt.text = heldWeapon.weaponData.weaponName;
        UIManager.Instance.ammoTxt.text = heldWeapon.currentMag + "/" + heldWeapon.currentReserve;
        heldWeapon.isReloading = false;
    }

    public void SwitchToNextWeapon()
    {
        int length = equippedWeapons.Length;
        // Look at each slot after the current
        for (int offset = 1; offset < length; offset++)
        {
            int index = (currentWeaponIndex + offset) % length;
            if (equippedWeapons[index] != null)
            {
                SwitchToWeapon(index);
                return;
            }
        }
        // no non-null weapon found → do nothing
    }
    public void SwitchToPreviousWeapon()
    {
        int length = equippedWeapons.Length;
        // Look backwards from the current
        for (int offset = 1; offset < length; offset++)
        {
            int index = (currentWeaponIndex - offset + length) % length;
            if (equippedWeapons[index] != null)
            {
                SwitchToWeapon(index);
                return;
            }
        }
        // no non-null weapon found → do nothing
    }

    public void SwitchToSentry(int index)
    {
        currentSentryIndex = index;
        heldSentry = equippedSentries[currentSentryIndex];

        if (heldSentry != null)
        {
            UIManager.Instance.buildStatusTxt.text = heldSentry.weaponName;
        }
    }

    public void SwitchToNextSentry()
    {
        int length = equippedSentries.Length;
        // Look at each slot after the current
        for (int offset = 1; offset < length; offset++)
        {
            int index = (currentSentryIndex + offset) % length;
            if (equippedSentries[index] != null)
            {
                SwitchToSentry(index);
                return;
            }
        }
        // no non-null weapon found → do nothing
    }

    public void SwitchToPreviousSentry()
    {
        int length = equippedSentries.Length;
        // Look backwards from the current
        for (int offset = 1; offset < length; offset++)
        {
            int index = (currentSentryIndex - offset + length) % length;
            if (equippedSentries[index] != null)
            {
                SwitchToSentry(index);
                return;
            }
        }
        // no non-null weapon found → do nothing
    }
}
