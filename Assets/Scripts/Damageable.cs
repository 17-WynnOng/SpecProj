using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;

    public delegate void OnDeath();
    public event OnDeath onDeath;

    [Header("Visual Feedback")]
    private Renderer[] objectRenderers;
    [SerializeField] private Color damageColor = Color.blue;
    [SerializeField] private float damageEffectDuration = 0.5f;

    private Coroutine damageCoroutine;
    private Color[] originalColors;
    private Material[] runtimeMaterials;

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        // Automatically get all child renderers, including nested ones
        objectRenderers = GetComponentsInChildren<Renderer>();

        List<Material> allMats = new List<Material>();
        List<Color> allOriginalColors = new List<Color>();

        foreach (var renderer in objectRenderers)
        {
            Material[] mats = renderer.materials; // unique material instances
            allMats.AddRange(mats);
            foreach (var mat in mats)
            {
                allOriginalColors.Add(mat.color);
            }
        }

        runtimeMaterials = allMats.ToArray();
        originalColors = allOriginalColors.ToArray();
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (objectRenderers != null)
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
            currentHealth = 0f;
        }
        else
        {
            IfDamagedByPlayer();
        }

    }

    protected virtual void Die()
    {
        onDeath?.Invoke(); // Notify listeners
        Destroy(gameObject); // Or pool
    }

    private IEnumerator FlashColor()
    {
        foreach (var mat in runtimeMaterials)
            mat.color = damageColor;

        float elapsedTime = 0f;
        while (elapsedTime < damageEffectDuration)
        {
            for (int i = 0; i < runtimeMaterials.Length; i++)
            {
                runtimeMaterials[i].color = Color.Lerp(damageColor, originalColors[i], elapsedTime / damageEffectDuration);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < runtimeMaterials.Length; i++)
        {
            runtimeMaterials[i].color = originalColors[i];
        }
    }

    public virtual void IfDamagedByPlayer() { }
}
