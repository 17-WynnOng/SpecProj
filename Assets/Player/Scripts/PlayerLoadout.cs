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
    private int currentWeaponIndex = 0;

    [Header("Held Sentry")]
    public Weapon heldSentry;

    [Tooltip("Prefabs for the two sentry types")]
    [SerializeField] private WeaponData[] equippedSentries;
    private int currentSentryIndex = 0; // which sentry prefab is selected

    [HideInInspector] public bool buildMode = false;

    public void LoadPlayerLoadout()
    {
        WeaponData primary = LoadoutManager.Instance.PrimaryWeapon;
        WeaponData secondary = LoadoutManager.Instance.SecondaryWeapon;
        equippedSentries = LoadoutManager.Instance.Sentries;

        EquipWeapon(0, primary);
        EquipWeapon(1, secondary);


        SwitchToWeapon(0); // Start with primary
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
    }

    public void SwitchToSentry(int index)
    {
        currentWeaponIndex = index;
        heldWeapon = equippedWeapons[currentWeaponIndex];
    }

    public void SwitchToNextWeapon()
    {
        int nextIndex = (currentWeaponIndex + 1) % equippedWeapons.Length;
        SwitchToWeapon(nextIndex);
    }

    public void SwitchToPreviousWeapon()
    {
        int prevIndex = (currentWeaponIndex - 1 + equippedWeapons.Length) % equippedWeapons.Length;
        SwitchToWeapon(prevIndex);
    }

    public void ToggleBuildMode()
    {
        buildMode = !buildMode;
        // optionally: swap UI, cursor, disable shooting/movement, etc.
    }

    public void SwitchToNextSentry()
    {
        int nextIndex = (currentSentryIndex + 1) % equippedSentries.Length;
        SwitchToWeapon(nextIndex);
    }

    public void SwitchToPreviousSentry()
    {
        int prevIndex = (currentSentryIndex - 1 + equippedSentries.Length) % equippedSentries.Length;
        SwitchToWeapon(prevIndex);
    }

    public void PlaceSentry(Vector3 position, Quaternion rotation)
    {
        if (currentSentryIndex < 0 || currentSentryIndex >= equippedSentries.Length)
            return;

        Instantiate(equippedSentries[currentSentryIndex].weaponPrefab, position, rotation);
    }
}
