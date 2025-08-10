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
    [Header("General Deployable Settings")]
    public string deployableName;
    public string deployableID;
    public string deployableDesc;
    public int deployCost;
    public GameObject deployablePrefab;
    public GameObject deployableIconPrefab;
    public GameObject deployableGhost;
    public Sprite deployable2DIcon;

    [Header("Defensive/Offensive deployables")]
    public float damage = 10f;

    [Header("Sentries")]
    public int magazineSize;
    public int maxAmmo;
    public float reloadTime;
    public float range = 100f;
    public float fireRate = 0.1f;
    public LayerMask hitLayers; // Layers the sentry can hit
    public LayerMask losLayers; //Layers that the sentry will interact with

    [Header("Placement Settings")]
    public PlacementType placementType = PlacementType.Ground;
    public Color validPlacement;
    public Color invalidPlacement;
}

