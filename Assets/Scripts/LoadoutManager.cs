using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutManager : MonoBehaviour
{
    public static LoadoutManager Instance;

    public WeaponData PrimaryWeapon { get; private set; }
    public WeaponData SecondaryWeapon { get; private set; }

    public WeaponData[] Sentries { get; private set;}

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetLoadout(WeaponData primary, WeaponData secondary, WeaponData[] sentries)
    {
        PrimaryWeapon = primary;
        SecondaryWeapon = secondary;
        Sentries = sentries;
    }
}
