using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseCurrentHealth;
    [SerializeField] private GameObject canvas;
    private void Awake()
    {
        baseCurrentHealth = baseMaxHealth;
        canvas.SetActive(true);
    }

    private void Update()
    {
        if(GameManager.Instance.isSectorClear)
        {
            canvas.SetActive(false);
        }
    }

    public void TakeDamage(float damage)
    {
        baseCurrentHealth -= damage;

        UIManager.Instance.UpdateBaseHealthBar(baseCurrentHealth, baseMaxHealth);   

        if (baseCurrentHealth <= 0f)
        {
            Debug.Log("Base dead, You Lose");
            baseCurrentHealth = 0f;

            //insert what happens when lose
            SceneManagement.Instance.LoadScene("LoseScene");
        }
    }
}
