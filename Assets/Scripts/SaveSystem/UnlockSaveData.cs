using System.Collections.Generic;

[System.Serializable]
public class UnlockSaveData
{
    public List<string> unlockedWeaponIDs = new();
    public List<string> unlockedDeployableIDs = new();
}
