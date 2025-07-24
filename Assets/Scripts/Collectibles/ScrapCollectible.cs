using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapCollectible : Collectible
{
    protected override void Collect(PlayerController player)
    {
        player.playerLoadout.AddScrap(5);
        UIManager.Instance.UpdateScrapCount(player.playerLoadout.currentScrap);
        Destroy(this.gameObject);
    }
}
