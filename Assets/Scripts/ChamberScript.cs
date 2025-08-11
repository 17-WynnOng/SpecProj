using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChamberScript : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    public void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.isSectorClear)
        {
            Time.timeScale = 0;
            GameManager.Instance.isExtracted = true;
            Cursor.lockState = CursorLockMode.None;
            UIManager.Instance.extractCanvas.SetActive(true);
            UIManager.Instance.sectorClearCanvas.SetActive(false);
        }
    }

    private void Start()
    {
        canvas.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.Instance.isSectorClear)
        {
            canvas.SetActive(true);
        }
    }
}
