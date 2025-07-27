using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoStation : Deployable
{
    [SerializeField] private int ammoDropsLeft;
    [SerializeField] private int maxAmmoDrops;
    [SerializeField] private GameObject ammoPrefab;
    [SerializeField] private TMP_Text AmmoTxt;
    [SerializeField] private Transform spawnLocation;

    private void Start()
    {
        ammoDropsLeft = maxAmmoDrops;

        if (AmmoTxt != null)
            AmmoTxt.text = ammoDropsLeft.ToString();
    }
    public override void InteractWithDeployable()
    {
        if (ammoDropsLeft <= 0) return;

        Instantiate(ammoPrefab, spawnLocation.position, spawnLocation.rotation);
        ammoDropsLeft--;

        if (AmmoTxt != null)
            AmmoTxt.text = ammoDropsLeft.ToString();
    }
}
