using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private RectTransform enemyHealthBar;
    [SerializeField] private GameObject canvas;
    [SerializeField] private float visibleDuration = 3f;

    private Coroutine hideCoroutine;
    private float maxBarWidth;
    private bool shouldLookAtCamera = false;
    private Transform camTransform;

    private Transform target;

    private void Awake()
    {
        maxBarWidth = enemyHealthBar.sizeDelta.x;
        if (canvas != null)
            canvas.SetActive(false);
    }

    private void Start()
    {
        camTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (!shouldLookAtCamera || camTransform == null) return;

        transform.LookAt(transform.position - camTransform.forward);
    }

    public void UpdateEnemyHealthBar(float enemyCurrentHP, float enemyMaxHP)
    {
        if (enemyHealthBar == null)
            return;

        float percent = Mathf.Clamp01(enemyCurrentHP / enemyMaxHP);
        Vector2 size = enemyHealthBar.sizeDelta;
        size.x = percent * maxBarWidth;
        enemyHealthBar.sizeDelta = size;

        ShowTemporarily();
    }

    private void ShowTemporarily()
    {
        if (canvas == null) return;

        canvas.SetActive(true);
        FaceCameraOnce();

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(visibleDuration);
        canvas.SetActive(false);
        hideCoroutine = null;
    }

    public void FaceCameraOnce()
    {
        shouldLookAtCamera = true;
        // Optional: stop billboarding after a short time
        CancelInvoke(nameof(StopLooking));
        Invoke(nameof(StopLooking), 3f); // matches your hide delay
    }

    private void StopLooking()
    {
        shouldLookAtCamera = false;
    }
}
