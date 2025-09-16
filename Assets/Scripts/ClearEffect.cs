using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ClearEffect : MonoBehaviour
{
    public float flashDuration = 0.3f;
    public Color flashColor = Color.yellow;

    private Image img;
    private Color originalColor;

    void Awake()
    {
        img = GetComponent<Image>();
        if (img != null)
            originalColor = img.color;
    }

    public void PlayFlash()
    {
        if (img != null)
            StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        img.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        img.color = originalColor;
    }
}
