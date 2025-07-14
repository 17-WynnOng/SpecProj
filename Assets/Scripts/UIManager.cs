using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TMP_Text gunTxt;
    public TMP_Text buildStatusTxt;
    public TMP_Text ammoTxt;
    public TMP_Text selectedPrimaryTxt;
    public TMP_Text selectedSecondaryTxt;
    public TMP_Text[] selectedSentriesTxt;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void UpdateSentryList(WeaponData[] sentries)
    {
        for (int i = 0; i < selectedSentriesTxt.Length; i++)
        {
            if (i < sentries.Length && sentries[i] != null)
                selectedSentriesTxt[i].text = sentries[i].weaponName;
            else
                selectedSentriesTxt[i].text = "—";    // or blank
        }
    }
}
