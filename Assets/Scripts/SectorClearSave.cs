using UnityEngine;

public class SectorClearSave : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.allowSpawning = false;
            GameManager.Instance.ClearAllEnemies();
        }
    }
}
