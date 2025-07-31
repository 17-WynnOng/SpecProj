using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponLoadout_Btn : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    private WeaponData weaponData;

    public void Initialize(WeaponData data)
    {
        weaponData = data;
        nameText.text = data.weaponName;
    }


    public void OnClick()
    {
        if (weaponData == null)
            return;

        if (weaponData.slotType == WeaponType.Primary)
        {
            LoadoutUI.Instance.SelectPrimary(weaponData);
        }
        else if (weaponData.slotType == WeaponType.Secondary)
        {
            LoadoutUI.Instance.SelectSecondary(weaponData);
        }
    }
}
