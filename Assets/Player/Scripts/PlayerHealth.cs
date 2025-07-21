using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Damageable
{
    protected override void Die()
    {
        Debug.Log("No health, dead");

        //Add in player specific death stuff here. Like UI popup and so on
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        RectTransform healthBar = UIManager.Instance.healthBar;

        if (healthBar != null)
        {
            float percent = Mathf.Clamp01(currentHealth / maxHealth);
            Vector2 size = healthBar.sizeDelta;
            size.x = percent * UIManager.Instance.GetHPMaxWidth(); // percent * original bar width
            healthBar.sizeDelta = size;
        }
    }
}
