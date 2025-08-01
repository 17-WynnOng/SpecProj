using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartingLoadout_UI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText, slotType;
    [SerializeField] private Image icon2D;
    private WeaponData weaponData;
    private DeployableData deployableData;

    public void InitializeWeapon(WeaponData data)
    {
        weaponData = data;
        nameText.text = data.weaponName;
        icon2D.sprite = data.weapon2DIcon;

        if (weaponData.slotType == WeaponType.Primary)
            slotType.text = "PRIMARY";
        else if (weaponData.slotType == WeaponType.Secondary)
            slotType.text = "SECONDARY";
    }

    public void InitializeDeployable(DeployableData data)
    {
        deployableData = data;
        nameText.text = deployableData.deployableName;
        icon2D.sprite = deployableData.deployable2DIcon;
        slotType.text = "DEPLOYABLE";
    }
}
