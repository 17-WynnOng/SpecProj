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
    public string deployableID;

    [Header("Defensive/Offensive deployables")]
    public float damage = 10f;

    [Header("Sentries")]
    public int magazineSize;
    public int maxAmmo;
    public int reloadTime;
    public float range = 100f;
    public float fireRate = 0.1f;
    public LayerMask hitLayers; // Layers the sentry can hit
    public LayerMask losLayers; //Layers that the sentry will interact with

    [Header("Placement Settings")]
    public PlacementType placementType = PlacementType.Ground;
    public Color validPlacement;
    public Color invalidPlacement;

    [Header("Loadout Button")]
    public GameObject loadoutButton;
}

