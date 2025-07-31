using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentDatabase", menuName = "Databases/EquipmentDatabase")]
public class EquipmentDatabase : ScriptableObject
{
    public List<WeaponData> allWeapons;

    public List<DeployableData> allDeployables;
}
