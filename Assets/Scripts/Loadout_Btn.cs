using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Loadout_Btn : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText, slotType;
    [SerializeField] private Image icon2D;
    [SerializeField] private Button button;
    private WeaponData weaponData;
    private DeployableData deployableData;

    private void Start()
    {
        button.onClick.AddListener(() =>
        {
            if (weaponData != null)
            {
                if (weaponData.slotType == WeaponType.Primary)
                {
                    LoadoutUI.Instance.SelectPrimary(weaponData);
                }
                else if (weaponData.slotType == WeaponType.Secondary)
                {
                    LoadoutUI.Instance.SelectSecondary(weaponData);
                }
            }

            if (deployableData != null)
            {
                LoadoutUI.Instance.SelectDeployables(deployableData);
            }
        });
    }
    public void InitializeWeapon(WeaponData data)
    {
        weaponData = data;
        deployableData = null;
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
        weaponData = null;
        nameText.text = deployableData.deployableName;
        icon2D.sprite = deployableData.deployable2DIcon;
        slotType.text = "DEPLOYABLE";
    }


    //public void OnClick()
    //{
    //    Debug.Log("Is clicked");

    //    if (weaponData != null)
    //    {
    //        if (weaponData.slotType == WeaponType.Primary)
    //        {
    //            LoadoutUI.Instance.SelectPrimary(weaponData);
    //        }
    //        else if (weaponData.slotType == WeaponType.Secondary)
    //        {
    //            LoadoutUI.Instance.SelectSecondary(weaponData);
    //        }
    //    }

    //    if (deployableData != null)
    //    {
    //        LoadoutUI.Instance.SelectDeployables(deployableData);
    //    }
    //}
}
