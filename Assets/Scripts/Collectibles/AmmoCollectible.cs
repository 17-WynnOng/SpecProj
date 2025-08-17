using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoCollectible : Collectible
{
    [SerializeField] private int AddAmmo;

    protected override void Collect(PlayerController player)
    {
        player.playerLoadout.AddAmmo(AddAmmo);
        AudioManager.Instance.PlayOneShotSFXByName("collectibles");
        Destroy(this.gameObject);
    }
}
