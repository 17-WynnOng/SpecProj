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
    [SerializeField] private float gridSize = 1f;

    public bool buildMode = false;

    private GameObject currentGhostInstance;

    private bool canPlace = true;

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
        if (!canPlace)
            return;

        if (loadout.currentScrap >= loadout.heldDeployable.deployCost)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            if (Physics.Raycast(ray, out var hit, buildDistance, buildableLayer))
            {
                loadout.currentScrap -= loadout.heldDeployable.deployCost;
                UIManager.Instance.UpdateScrapCount(loadout.currentScrap);

                var data = loadout.heldDeployable;
                Vector3 snappedPos = SnapToLocalGrid(hit.point, hit.normal);
                Deployable.PlaceDeployable(snappedPos, hit.normal, data);
                UIManager.Instance.UpdateDeployableCost(loadout.currentScrap, loadout.heldDeployable.deployCost);
            }
        }
    }

    public void TryBuildToggle(PlayerLoadout playerLoadout)
    {
        buildMode = !buildMode;

        if (buildMode)
        {
            // disable current weapon
            if (playerLoadout.heldWeapon != null)
            {
                playerLoadout.heldWeapon.gameObject.SetActive(false);
                playerLoadout.heldWeapon = null;
                playerLoadout.SwitchToDeployable(playerLoadout.currentDeployableIndex);
            }

            SwitchGhost(playerLoadout);
        }
        else
        {
            if (currentGhostInstance != null)
            {
                Destroy(currentGhostInstance);
            }
            // re-equip last weapon slot
            playerLoadout.SwitchToWeapon(playerLoadout.currentWeaponIndex);
            playerLoadout.HideBuildTool();
        }
    }

    public void UpdateGhostPosition(Camera cam, PlayerLoadout playerloadout)
    {
        if (!buildMode || currentGhostInstance == null)
            return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out var hit, buildDistance, buildableLayer))
        {
            Vector3 snapped = SnapToLocalGrid(hit.point, hit.normal);
            currentGhostInstance.transform.position = snapped;


            switch (playerloadout.heldDeployable.placementType)
            {
                case PlacementType.Wall:
                    currentGhostInstance.transform.rotation = Quaternion.LookRotation(hit.normal); // face outward from wall
                    break;

                case PlacementType.Ground:
                default:
                    currentGhostInstance.transform.rotation = Quaternion.LookRotation(Vector3.forward); // default forward
                    break;
            }

            // ==== Overlap check ====
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

            Renderer[] renderers = currentGhostInstance.GetComponentsInChildren<Renderer>();
            Color color = canPlace ? playerloadout.heldDeployable.validPlacement : playerloadout.heldDeployable.invalidPlacement;

            foreach (Renderer rend in renderers)
            {
                rend.material.color = color;
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

    private Vector3 SnapToLocalGrid(Vector3 hitPoint, Vector3 hitNormal)
    {
        //Get basis vectors
        Vector3 up = hitNormal.normalized;
        Vector3 right = Vector3.Cross(up, Vector3.forward);
        if (right.sqrMagnitude < 0.001f)
            right = Vector3.Cross(up, Vector3.right); // fallback if up is parallel to forward

        Vector3 forward = Vector3.Cross(right, up);

        //Build rotation matrix
        Matrix4x4 toLocal = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(forward, up), Vector3.one).inverse;
        Matrix4x4 toWorld = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(forward, up), Vector3.one);

        //Transform to local grid space
        Vector3 local = toLocal.MultiplyPoint3x4(hitPoint);

        //Snap X and Z in local space
        local.x = Mathf.Round(local.x / gridSize) * gridSize;
        local.z = Mathf.Round(local.z / gridSize) * gridSize;

        //Transform back to world space
        return toWorld.MultiplyPoint3x4(local);
    }

}
