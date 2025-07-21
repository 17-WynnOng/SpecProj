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

        RectTransform baseHealthBar = UIManager.Instance.baseHealthBar;

        if (baseHealthBar != null)
        {
            float percent = Mathf.Clamp01(baseCurrentHealth / baseMaxHealth);
            Vector2 size = baseHealthBar.sizeDelta;
            size.x = percent * UIManager.Instance.GetBaseHPMaxWidth(); // percent * original bar width
            baseHealthBar.sizeDelta = size;
        }

        if (baseCurrentHealth <= 0f)
        {
            Debug.Log("Base dead, You Lose");
            baseCurrentHealth = 0f;

            //insert what happens when lose
        }
    }
}
