using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseCurrentHealth;

    private void Awake()
    {
        baseCurrentHealth = baseMaxHealth;
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
