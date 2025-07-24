using UnityEngine;

public abstract class Deployable : MonoBehaviour
{

    protected DeployableData deployableData;

    public void Initialize(DeployableData data)
    {
        deployableData = data;
    }

    public static void PlaceDeployable(Vector3 hitPoint, Vector3 surfaceNormal, DeployableData data)
    {
        if (data == null || data.deployablePrefab == null)
        {
            Debug.LogWarning("Missing deployable prefab");
            return;
        }

        Quaternion rotation;

        switch (data.placementType)
        {
            case PlacementType.Wall:
                rotation = Quaternion.LookRotation(surfaceNormal); // face outward from wall
                break;
            case PlacementType.Ground:
            default:
                rotation = Quaternion.LookRotation(Vector3.forward); // default forward
                break;
        }

        Vector3 position = hitPoint;

        GameObject instance = GameObject.Instantiate(data.deployablePrefab, position, rotation);
        Deployable deployable = instance.GetComponent<Deployable>();

        if (deployable != null)
            deployable.Initialize(data);
    }
}
