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

    [Header("Build Tool")]
    [SerializeField] private GameObject buildTool;
    private GameObject instantiatedBuildTool;
    private Transform buildToolCanvas;
    private Dictionary<DeployableData, GameObject> iconDic = new();

    [Header("Held Deployable")]
    public DeployableData heldDeployable;

    [Tooltip("Prefabs for deployables")]
    [SerializeField] public DeployableData[] equippedDeployables;
    [HideInInspector] public int currentDeployableIndex = 0;

    [Header("Build Resources")]
    public int currentScrap = 250;

    public void LoadPlayerLoadout()
    {
        WeaponData primary = LoadoutManager.Instance.primaryWeapon;
        EquipWeapon(0, primary);

        WeaponData secondary = LoadoutManager.Instance.secondaryWeapon;

        if (secondary != null)
            EquipWeapon(1, secondary);

        equippedDeployables = LoadoutManager.Instance.deployables;
        EquipBuildTool();

        SwitchToWeapon(0); // Start with primary
        SwitchToDeployable(0);

        HideBuildTool();
        UIManager.Instance.UpdateScrapCount(currentScrap);
    }

    private void EquipWeapon(int slot, WeaponData data)
    {
        Weapon weapon = Instantiate(data.weaponPrefab, weaponHolder).GetComponent<Weapon>();
        weapon.Initialize(data, Camera.main);
        equippedWeapons[slot] = weapon;
    }

    private void EquipBuildTool()
    {
        instantiatedBuildTool = Instantiate(buildTool, weaponHolder);
        UIManager.Instance.InitializeBuildToolUI(instantiatedBuildTool);
        instantiatedBuildTool.transform.localRotation = Quaternion.identity;

        buildToolCanvas = instantiatedBuildTool.transform.Find("Canvas/BuildMode_UI");
        if (buildToolCanvas == null)
        {
            Debug.LogWarning("BuildTool canvas not found!");
            return;
        }

        //instantiate all icons
        iconDic.Clear();
        foreach (var deployable in equippedDeployables)
        {
            if (deployable == null || deployable.deployableIconPrefab == null)
                continue;

            GameObject icon = Instantiate(deployable.deployableIconPrefab, buildToolCanvas);
            icon.transform.localPosition = new Vector3(0, -0.044f, 0);
            icon.SetActive(false);
            iconDic.Add(deployable, icon);
        }

    }

    public void SwitchToBuildTool()
    {
        if (instantiatedBuildTool != null)
            instantiatedBuildTool.SetActive(true);

        if (heldDeployable == null)
        {
            UIManager.Instance.UpdateDeployableCost(currentScrap, 0);
        }
    }

    public void HideBuildTool()
    {
        if (instantiatedBuildTool != null)
            instantiatedBuildTool.SetActive(false);
    }

    public void SwitchToWeapon(int index)
    {
        for (int i = 0; i < equippedWeapons.Length; i++)
            equippedWeapons[i]?.gameObject.SetActive(i == index);

        currentWeaponIndex = index;
        heldWeapon = equippedWeapons[currentWeaponIndex];
        UIManager.Instance.gunTxt.text = heldWeapon.weaponData.weaponName;
        UIManager.Instance.magazineTxt.text = heldWeapon.currentMag.ToString();
        UIManager.Instance.reserveAmmoTxt.text = heldWeapon.currentReserve.ToString();
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
    }

    public void SwitchToDeployable(int index)
    {
        SwitchToBuildTool();
        currentDeployableIndex = index;
        heldDeployable = equippedDeployables[currentDeployableIndex];

        if (heldDeployable != null)
        {
            UIManager.Instance.UpdateDeployableCost(currentScrap, heldDeployable.deployCost);
            UIManager.Instance.UpdateHeldDeployable(heldDeployable.deployableName);
            PlayerBuild.Instance.SwitchGhost(this);

            // Hide all indicators
            foreach (var kvp in iconDic)
                kvp.Value.SetActive(false);

            // Show the one for the current deployable
            if (iconDic.TryGetValue(heldDeployable, out var icon))
            {
                icon.SetActive(true);
            }
        }
    }

    public void SwitchToNextDeployable()
    {
        int length = equippedDeployables.Length;
        // Look at each slot after the current
        for (int offset = 1; offset < length; offset++)
        {
            int index = (currentDeployableIndex + offset) % length;
            if (equippedDeployables[index] != null)
            {
                SwitchToDeployable(index);
                return;
            }
        }
        // no non-null weapon found → do nothing
    }

    public void SwitchToPreviousDeployable()
    {
        int length = equippedDeployables.Length;
        // Look backwards from the current
        for (int offset = 1; offset < length; offset++)
        {
            int index = (currentDeployableIndex - offset + length) % length;
            if (equippedDeployables[index] != null)
            {
                SwitchToDeployable(index);
                return;
            }
        }
        // no non-null weapon found → do nothing
    }

    public void AddScrap(int amount)
    {
        currentScrap += amount;
    }

    public void AddAmmo(int amount)
    {
        if (heldWeapon != null)
        {
            heldWeapon.currentReserve += amount;
            UIManager.Instance.UpdateAmmoUI(heldWeapon.currentMag, heldWeapon.currentReserve);
        }
    }
}
