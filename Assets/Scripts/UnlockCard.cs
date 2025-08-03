using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnlockCard : MonoBehaviour
{
    [SerializeField] private GameObject front;
    [SerializeField] private GameObject back;

    [SerializeField] private Animator animator;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text cardName, cardDesc;
    [SerializeField] private Image cardIcon;

    [SerializeField] private object assignedData;

    private UnlockCardSpawner spawner;

    private GameObject continueBtn;

    private bool hasFlipped = false;

    public void Start()
    {
        front.SetActive(false);
        back.SetActive(true);

        button.onClick.AddListener(() =>
        {
            if (assignedData is WeaponData w)
            {
                LoadoutManager.Instance.UnlockWeapon(w.weaponID);
            }
            else if (assignedData is DeployableData d)
            {
                LoadoutManager.Instance.UnlockDeployable(d.deployableID);
            }
        });
    }

    public void OnClick()
    {
        if (hasFlipped) return;
        hasFlipped = true;

        button.interactable = false;
        animator.SetTrigger("Flip");
        spawner.DisableAllCards(this);
        continueBtn.SetActive(true);
    }

    public void InitializeSpawner(UnlockCardSpawner spawner)
    {
        this.spawner = spawner;
    }

    public void AssignWeapon(WeaponData weapon)
    {
        assignedData = weapon;
        cardIcon.sprite = weapon.weapon2DIcon;
        cardName.text = weapon.weaponName;
    }

    public void AssignContinueBtn(GameObject btn)
    {
        continueBtn = btn;
    }

    public void AssignDeployable(DeployableData deployable)
    {
        assignedData = deployable;
        cardIcon.sprite = deployable.deployable2DIcon;
        cardName.text = deployable.deployableName;
    }

    public string GetName()
    {
        if (assignedData is WeaponData w) 
            return w.weaponName;

        if (assignedData is DeployableData d) 
            return d.deployableName;

        return "Unknown";
    }

    public void DisableButton()
    {
        button.interactable = false;
    }    
}
