using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Damageable
{
    public bool isDead;
    protected override void Die()
    {
        Debug.Log("No health, dead");

        //Add in player specific death stuff here. Like UI popup and so on
        SceneManagement.Instance.LoadScene("LoseScene");
    }

    public override void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
            currentHealth = 0f;
        }

        UIManager.Instance.UpdatePlayerHealthBar(currentHealth, maxHealth);
    }
}
