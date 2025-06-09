using UnityEngine;
public enum FireMode
{
    SemiAuto,
    FullAuto,
}

[CreateAssetMenu(fileName = "NewWeaponData", menuName =
"Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public GameObject weaponPrefab;
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.1f;
    public LayerMask hitLayers; // Layers the weapon can hit

    [Header("Firing Mode")]
    public FireMode fireMode;
}

