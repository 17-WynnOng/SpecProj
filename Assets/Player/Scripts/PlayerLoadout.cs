using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLoadout : MonoBehaviour
{

    [SerializeField] private Transform weaponHolder;

    [Header("Held Weapon")]
    public Weapon heldWeapon;

    [SerializeField] private Weapon[] equippedWeapons = new Weapon[2];
    private int currentWeaponIndex = 0;

    public void LoadPlayerLoadout()
    {
        WeaponData primary = LoadoutManager.Instance.PrimaryWeapon;
        WeaponData secondary = LoadoutManager.Instance.SecondaryWeapon;

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
}
