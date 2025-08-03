using TMPro;
using UnityEngine;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private TMP_Text textComponent;
    [SerializeField][TextArea] private string fullText;
    [SerializeField] private float delay = 0.05f;

    [Tooltip("Only use this if you need something to appear or disappear at the end of the typewriter")]
    [SerializeField] private GameObject endText;

    private Coroutine typeCoroutine;
    private bool isSkipping = false;

    private void OnEnable()
    {
        ResetTypewriter(); // Clear & restart when enabled
        typeCoroutine = StartCoroutine(TypeText());
    }

    private void OnDisable()
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
        }

        // Optional: reset visuals here too
        if (textComponent != null)
            textComponent.text = "";

        if (endText != null)
            endText.SetActive(false);
    }

    private void ResetTypewriter()
    {
        isSkipping = false;

        if (textComponent != null)
            textComponent.text = "";

        if (endText != null)
            endText.SetActive(false);
    }

    private IEnumerator TypeText()
    {
        textComponent.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            if (isSkipping)
            {
                textComponent.text = fullText;
                break;
            }

            textComponent.text += fullText[i];
            yield return new WaitForSeconds(delay);
        }

        if (endText != null)
        {
            endText.SetActive(!endText.activeSelf);
        }

        yield break;
    }

    public void SkipTypeWriter()
    {
        isSkipping = true;
    }
}
