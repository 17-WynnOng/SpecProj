using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutUI : MonoBehaviour
{
    public static LoadoutUI Instance;

    private WeaponData selectedPrimary;
    private WeaponData selectedSecondary;
    private DeployableData[] selectedDeployables;

    [SerializeField] private Transform content;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (selectedDeployables == null || selectedDeployables.Length != 4)
            selectedDeployables = new DeployableData[4];
    }

    private void Start()
    {
        foreach (var weapon in LoadoutManager.Instance.unlockedWeapons)
        {
            if (weapon.slotType == WeaponType.Primary || weapon.slotType == WeaponType.Secondary)
            {
                GameObject btn = Instantiate(weapon.loadoutButton, content);
                btn.GetComponent<Loadout_Btn>().InitializeWeapon(weapon);
            }
        }

        foreach (var deployable in LoadoutManager.Instance.unlockedDeployables)
        {
            GameObject btn = Instantiate(deployable.loadoutButton, content);
            btn.GetComponent<Loadout_Btn>().InitializeDeployable(deployable);
        }
    }

    public void SelectPrimary(WeaponData weapon)
    {
        selectedPrimary = weapon;
        Debug.Log("Primary Selected: " + weapon.weaponName);
        UIManager.Instance.selectedPrimaryTxt.text = weapon.weaponName;
    }

    public void SelectSecondary(WeaponData weapon)
    {
        selectedSecondary = weapon;
        Debug.Log("Secondary Selected: " + weapon.weaponName);
        UIManager.Instance.selectedSecondaryTxt.text = weapon.weaponName;
    }

    public void SelectDeployables(DeployableData deployable)
    {
        // Prevent duplicates
        for (int i = 0; i < selectedDeployables.Length; i++)
        {
            if (selectedDeployables[i] == deployable)
            {
                Debug.LogWarning($"Deployable '{deployable.deployableName}' is already selected.");
                return;
            }
        }

        for (int i = 0; i < selectedDeployables.Length; i++)
        {
            if (selectedDeployables[i] == null)
            {
                selectedDeployables[i] = deployable;
                Debug.Log($"Added to slot {i}: {deployable.deployableName}");
                UIManager.Instance.UpdateSentryList(selectedDeployables);
                return;
            }
        }

        // shift everything right
        for (int i = selectedDeployables.Length - 1; i > 0; i--)
            selectedDeployables[i] = selectedDeployables[i - 1];

        // put the new weapon in the first slot
        selectedDeployables[0] = deployable;
        Debug.Log($"Slots full → pushed down and placed new in slot 0: {deployable.deployableName}");
        UIManager.Instance.UpdateSentryList(selectedDeployables);
    }

    public void ConfirmLoadout()
    {
        if (selectedPrimary != null)
        {
            LoadoutManager.Instance.SetLoadout(selectedPrimary, selectedSecondary, selectedDeployables);
                
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
            UIManager.Instance.loadoutUICanvas.SetActive(false);
            UIManager.Instance.gameUICanvas.SetActive(true);
            GameManager.Instance.StartWaveCountdown();
        }
        else
        {
            Debug.Log("Unable to deploy without full loadout");
        }
    }
}
