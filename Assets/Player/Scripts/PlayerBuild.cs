using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuild : MonoBehaviour
{
    [SerializeField] private float buildDistance = 100f;

    [Header("Build Mode")]
    [SerializeField] private LayerMask buildableLayer; // e.g. “Ground”

    [Header("Grid Settings")]
    [SerializeField] private float gridSize = 1f;

    public bool buildMode = false;

    private GameObject currentGhostInstance;


    public void TryPlaceDeployable(Camera playerCamera, PlayerLoadout loadout)
    {
        if (loadout.currentScrap >= loadout.heldDeployable.deployCost)
        {
            loadout.currentScrap -= loadout.heldDeployable.deployCost;
            UIManager.Instance.UpdateScrapCount(loadout.currentScrap);

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
            if (Physics.Raycast(ray, out var hit, buildDistance, buildableLayer))
            {
                var data = loadout.heldDeployable;
                Vector3 snappedPos = SnapToLocalGrid(hit.point, hit.normal);
                Deployable.PlaceDeployable(snappedPos, hit.normal, data);
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

            if (playerLoadout.heldDeployable.deployableGhost != null)
            {
                currentGhostInstance = Instantiate(playerLoadout.heldDeployable.deployableGhost);
            }
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
