using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuild : MonoBehaviour
{
    public static PlayerBuild Instance;

    [SerializeField] private float buildDistance = 100f;

    [Header("Build Mode")]
    [SerializeField] private LayerMask buildableLayer; // e.g. “Ground”

    [Header("Grid Settings")]
    [SerializeField] private float gridSizeHorizontal = 1f; // used for ground
    [SerializeField] private float gridSizeVertical = 0.5f; // used for walls

    public bool buildMode = false;
    public bool sellMode = false;

    private GameObject currentGhostInstance;

    private bool canPlace = true;

    private Vector3 cachedPlacementPos;
    private Vector3 cachedPlacementNormal;
    private bool placementValid = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TryPlaceDeployable(Camera playerCamera, PlayerLoadout loadout)
    {
        if (!placementValid)
            return;

        if (loadout.currentScrap < loadout.heldDeployable.deployCost)
            return;

        loadout.currentScrap -= loadout.heldDeployable.deployCost;
        UIManager.Instance.UpdateScrapCount(loadout.currentScrap);

        Deployable.PlaceDeployable(cachedPlacementPos, cachedPlacementNormal, loadout.heldDeployable);
        UIManager.Instance.UpdateDeployableCost(loadout.currentScrap, loadout.heldDeployable.deployCost);
    }

    public void TrySellDeployable(Camera playerCamera, PlayerLoadout loadout)
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, buildDistance))
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj.CompareTag("Deployable"))
            {
                Deployable deployable = hitObj.GetComponent<Deployable>();
                if (deployable != null)
                {
                    // Refund scrap to player
                    int refundAmount = deployable.deployableData.deployCost;
                    loadout.currentScrap += refundAmount;

                    UIManager.Instance.UpdateScrapCount(loadout.currentScrap);
                    Destroy(hitObj);
                }
            }
        }
    }

    public void UpdateSellTarget(Camera cam)
    {
        if (!sellMode) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, buildDistance))
        {
            GameObject hitObj = hit.collider.gameObject;
            if (hitObj.CompareTag("Deployable"))
            {
                Deployable deployable = hitObj.GetComponent<Deployable>();
                if (deployable != null)
                {
                    UIManager.Instance.UpdateRecycleCost(deployable.deployableData.deployCost);
                    return;
                }
            }
        }

        // No valid deployable found
        UIManager.Instance.UpdateRecycleCost(0);
    }

    public void ToggleBuildMode(PlayerLoadout playerLoadout)
    {
        buildMode = !buildMode;

        if (buildMode)
        {
            UIManager.Instance.DisableBuildToolUI();

            if (playerLoadout.heldWeapon != null)
            {
                playerLoadout.heldWeapon.gameObject.SetActive(false);
                playerLoadout.heldWeapon = null;
            }

            UIManager.Instance.EnableBuildUI();
            playerLoadout.SwitchToDeployable(playerLoadout.currentDeployableIndex);
            SwitchGhost(playerLoadout);

            if (playerLoadout.heldDeployable != null)
            {
                UIManager.Instance.UpdateHeldDeployable(playerLoadout.heldDeployable.deployableName);
                UIManager.Instance.UpdateDeployableCost(playerLoadout.currentScrap, playerLoadout.heldDeployable.deployCost);
            }
        }
        else
        {
            if (currentGhostInstance != null)
                Destroy(currentGhostInstance);

            // re-equip last weapon slot
            playerLoadout.SwitchToWeapon(playerLoadout.currentWeaponIndex);
            playerLoadout.HideBuildTool();
        }
    }

    public void ToggleSellMode(PlayerLoadout playerLoadout)
    {
        sellMode = !sellMode;

        if (sellMode)
        {
            // Disable ghost and enable sell UI
            if (currentGhostInstance != null)
                currentGhostInstance.SetActive(false);

            UIManager.Instance.EnableSellUI();
        }
        else
        {

            if (currentGhostInstance != null)
                currentGhostInstance.SetActive(true);

            // Return to build UI (and restore ghost)
            UIManager.Instance.EnableBuildUI();
        }
    }

    public void UpdateGhostPosition(Camera cam, PlayerLoadout playerloadout)
    {
        placementValid = false;

        if (!buildMode || currentGhostInstance == null)
            return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray, buildDistance);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            GameObject hitObj = hit.collider.gameObject;

            // Skip self and ghost
            if (hitObj.layer == LayerMask.NameToLayer("IgnoreRaycast") || hitObj.CompareTag("Deployable"))
                continue;

            // Block ray if a wall is in front and we're placing a ground object
            if (hitObj.layer == LayerMask.NameToLayer("Environment"))
            {
                if (playerloadout.heldDeployable.placementType != PlacementType.Wall)
                    break; // wall blocks ray for ground deployables
            }

            // Not a wall, check if it's buildable
            if (((1 << hitObj.layer) & buildableLayer) == 0)
                continue;

            // Now filter by placement type using surface normal
            Vector3 normal = hit.normal;
            switch (playerloadout.heldDeployable.placementType)
            {
                case PlacementType.Ground:
                    if (Vector3.Dot(normal, Vector3.up) < 0.75f)
                        continue;
                    break;

                case PlacementType.Wall:
                    float upDot = Mathf.Abs(Vector3.Dot(normal, Vector3.up));
                    if (upDot > 0.5f)
                        continue;
                    break;
            }

            HandleGhostPlacement(hit, playerloadout);
            break;
        }
    }

    private void HandleGhostPlacement(RaycastHit hit, PlayerLoadout playerloadout)
    {
        Vector3 snapped = SnapToLocalGrid(hit.point, hit.normal, playerloadout);
        currentGhostInstance.transform.position = snapped;

        switch (playerloadout.heldDeployable.placementType)
        {
            case PlacementType.Wall:
                currentGhostInstance.transform.rotation = Quaternion.LookRotation(hit.normal);
                break;

            case PlacementType.Ground:
            default:
                currentGhostInstance.transform.rotation = Quaternion.LookRotation(Vector3.forward);
                break;
        }

        Bounds bounds = currentGhostInstance.GetComponent<Collider>().bounds;
        Collider[] overlaps = Physics.OverlapBox(bounds.center, bounds.extents, currentGhostInstance.transform.rotation);
        bool overlappingSentry = false;
        foreach (var col in overlaps)
        {
            if (col.gameObject.CompareTag("Deployable") && col.gameObject != currentGhostInstance)
            {
                overlappingSentry = true;
                break;
            }
        }

        canPlace = !overlappingSentry;
        bool canAfford = playerloadout.currentScrap >= playerloadout.heldDeployable.deployCost;
        placementValid = (canAfford && canPlace);

        if (placementValid)
        {
            cachedPlacementPos = snapped;
            cachedPlacementNormal = hit.normal;
        }

        Color color = (canAfford && canPlace) ? playerloadout.heldDeployable.validPlacement : playerloadout.heldDeployable.invalidPlacement;

        Renderer[] renderers = currentGhostInstance.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                mat.color = color;
            }
        }
    }

    public void SwitchGhost(PlayerLoadout playerLoadout)
    {
        if (!buildMode)
            return;

        if (currentGhostInstance != null)
        {
            Destroy(currentGhostInstance);
        }

        if (playerLoadout.heldDeployable != null && playerLoadout.heldDeployable.deployableGhost != null)
        {
            currentGhostInstance = Instantiate(playerLoadout.heldDeployable.deployableGhost);
        }
    }

    private Vector3 SnapToLocalGrid(Vector3 hitPoint, Vector3 hitNormal, PlayerLoadout playerloadout)
    {
        float gridSize = 1f;

        // Determine grid size based on placement type
        if (playerloadout != null && playerloadout.heldDeployable != null)
        {
            switch (playerloadout.heldDeployable.placementType)
            {
                case PlacementType.Ground:
                    gridSize = gridSizeHorizontal;
                    break;
                case PlacementType.Wall:
                    gridSize = gridSizeVertical;
                    break;
            }
        }

        // Get surface basis
        Vector3 up = hitNormal.normalized;
        Vector3 right = Vector3.Cross(up, Vector3.forward);
        if (right.sqrMagnitude < 0.001f)
            right = Vector3.Cross(up, Vector3.right); // fallback

        Vector3 forward = Vector3.Cross(right, up);

        Matrix4x4 toLocal = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(forward, up), Vector3.one).inverse;
        Matrix4x4 toWorld = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(forward, up), Vector3.one);

        Vector3 local = toLocal.MultiplyPoint3x4(hitPoint);

        // Snap X and Z in local space
        local.x = Mathf.Round(local.x / gridSize) * gridSize;
        local.z = Mathf.Round(local.z / gridSize) * gridSize;

        return toWorld.MultiplyPoint3x4(local);
    }
}
