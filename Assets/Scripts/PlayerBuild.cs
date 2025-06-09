using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuild : MonoBehaviour
{
    [SerializeField] private float buildDistance = 100f;

    [Header("Build Mode")]
    [SerializeField] private LayerMask buildableLayer; // e.g. “Ground”

    public bool buildMode = false;


    public void TryPlaceSentry(Camera playerCamera, PlayerLoadout loadout)
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        if (Physics.Raycast(ray, out var hit, buildDistance, buildableLayer))
        {
            PlaceSentry(hit.point, Quaternion.LookRotation(transform.forward), loadout);
        }
    }

    public void PlaceSentry(Vector3 position, Quaternion rotation, PlayerLoadout loadout)
    {
        if (loadout.currentSentryIndex < 0 || loadout.currentSentryIndex >= loadout.equippedSentries.Length)
            return;

        Weapon sentry = Instantiate(loadout.heldSentry.weaponPrefab, position, rotation).GetComponent<Weapon>();
        sentry.Initialize(loadout.heldSentry);
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
                playerLoadout.SwitchToSentry(playerLoadout.currentSentryIndex);
            }
        }
        else
        {
            // re-equip last weapon slot
            playerLoadout.SwitchToWeapon(playerLoadout.currentWeaponIndex);
        }
    }
}
