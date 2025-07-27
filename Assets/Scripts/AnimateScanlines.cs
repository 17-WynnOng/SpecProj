using UnityEngine;
using UnityEngine.UI;

public class AnimateScanlines : MonoBehaviour
{
    [SerializeField] private RawImage img;
    void Update()
    {
        if (img != null)
        {
            Rect uv = img.uvRect;
            uv.y += Time.deltaTime * 0.01f;
            img.uvRect = uv;
        }
    }
}
