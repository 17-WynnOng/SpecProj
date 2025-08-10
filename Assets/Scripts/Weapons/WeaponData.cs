using UnityEngine;
public enum FireMode
{
    SemiAuto,
    FullAuto,
}

public enum WeaponType
{
    Primary,
    Secondary,
}

[CreateAssetMenu(fileName = "NewWeaponData", menuName =
"Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("General Weapoons settings")]
    public string weaponName;
    public string weaponID;
    public string weaponDesc;
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.1f;
    public Sprite weapon2DIcon;
    public GameObject weapon3DModel;
    public GameObject weaponPrefab;
    public WeaponType slotType;

    [Header("Weapon")]
    public int magazineSize;
    public int maxAmmo;
    public float reloadTime;
    public float recoil;

    [Header("Firing Settings")]
    public FireMode fireMode;
    public LayerMask hitLayers; // Layers the weapon can hit
}

