using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public delegate void OnDeath();
    public event OnDeath onDeath;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Color damageColor = Color.blue;
    [SerializeField] private float damageEffectDuration = 0.5f;

    private Coroutine damageCoroutine;
    private Color originalColor;
    private Material runtimeMaterial;

    private void Start()
    {
        currentHealth = maxHealth;

        // Create a unique instance of the material
        runtimeMaterial = objectRenderer.material;
        originalColor = runtimeMaterial.color;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (objectRenderer != null)
        {
            if (damageCoroutine != null)
            {
                StartCoroutine(FlashColor());
            }
            damageCoroutine = StartCoroutine(FlashColor());
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        onDeath?.Invoke(); // Notify listeners
        Destroy(gameObject); // Or pool
    }

    private IEnumerator FlashColor()
    {
        // Set to damage color instantly
        objectRenderer.material.color = damageColor;
        // Gradually transition back to the original color over time
        float elapsedTime = 0f;
        while (elapsedTime < damageEffectDuration)
        {
            objectRenderer.material.color = Color.Lerp(damageColor,
            originalColor, elapsedTime / damageEffectDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // Ensure the final color is reset to the original
        objectRenderer.material.color = originalColor;
    }
}
