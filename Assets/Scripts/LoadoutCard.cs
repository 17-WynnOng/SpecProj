using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutCard : MonoBehaviour
{
    [SerializeField] private GameObject front;
    [SerializeField] private GameObject back;

    [SerializeField] private Button button;

    public void Start()
    {
        front.SetActive(false);
        back.SetActive(true);
    }

    public void DisableButton()
    {
        button.interactable = false;
    }    
}
