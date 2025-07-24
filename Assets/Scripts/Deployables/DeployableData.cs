using UnityEngine;

public enum PlacementType
{
    Ground,
    Wall,
}

[CreateAssetMenu(fileName = "NewDeployableData", menuName =
"Deployable/DeployableData")]
public class DeployableData : ScriptableObject
{
    [Header("All deployables")]
    public int deployCost;
    public string deployableName;
    public GameObject deployablePrefab;
    public GameObject deployableIconPrefab;
    public GameObject deployableGhost;

    [Header("Defensive/Offensive deployables")]
    public float damage = 10f;


    [Header("Sentries")]
    public int magazineSize;
    public int maxAmmo;
    public int reloadTime;
    public float range = 100f;
    public float fireRate = 0.1f;

    [Header("Placement Settings")]
    public PlacementType placementType = PlacementType.Ground;

    public LayerMask hitLayers; // Layers the weapon can hit
}

