using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutManager : MonoBehaviour
{
    public static LoadoutManager Instance;

    [SerializeField] private EquipmentDatabase equipmentDB;

    public WeaponData primaryWeapon { get; private set; }
    public WeaponData secondaryWeapon { get; private set; }

    public DeployableData[] deployables { get; private set;}

    public List<WeaponData> unlockedWeapons = new List<WeaponData>();
    public List<DeployableData> unlockedDeployables = new List<DeployableData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (deployables == null || deployables.Length != 4)
            deployables = new DeployableData[4];

        LoadUnlockedData();
    }

    public void UnlockWeapon(string weaponID)
    {
        var weapon = equipmentDB.allWeapons.Find(w => w.weaponID == weaponID);
        if (weapon != null && !unlockedWeapons.Contains(weapon))
        {
            unlockedWeapons.Add(weapon);
            SaveUnlockedData();
        }
    }

    public void UnlockDeployable(string deployableID)
    {
        var deployable = equipmentDB.allDeployables.Find(d => d.deployableID == deployableID);
        if (deployable != null && !unlockedDeployables.Contains(deployable))
        {
            unlockedDeployables.Add(deployable);
            SaveUnlockedData();
        }
    }

    private void SaveUnlockedData()
    {
        var saveData = new UnlockSaveData();

        saveData.unlockedWeaponIDs = unlockedWeapons.ConvertAll(w => w.weaponID);
        saveData.unlockedDeployableIDs = unlockedDeployables.ConvertAll(d => d.deployableID);

        SaveSystem.Save("unlocks.json", saveData);
    }

    public void LoadUnlockedData()
    {
        if (!SaveSystem.Exists("unlocks.json"))
        {
            Debug.Log("No save found. Starting fresh.");
            return;
        }

        var loadedData = SaveSystem.Load<UnlockSaveData>("unlocks.json");

        unlockedWeapons = equipmentDB.allWeapons.FindAll(w => loadedData.unlockedWeaponIDs.Contains(w.weaponID));

        unlockedDeployables = equipmentDB.allDeployables.FindAll(d => loadedData.unlockedDeployableIDs.Contains(d.deployableID));
    }


    public void DeleteUnlockedData()
    {
        SaveSystem.Delete("unlocks.json");
    }

    public void SetLoadout(WeaponData selectedPrimary, WeaponData selectedSecondary, DeployableData[] selectedDeployables)
    {
        primaryWeapon = selectedPrimary;
        secondaryWeapon = selectedSecondary;
        deployables = selectedDeployables;
    }
}
