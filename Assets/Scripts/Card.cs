using UnityEngine;
using System;
using System.Collections;

public class Card : MonoBehaviour
{
    public int cardId;
    public Sprite frontSprite;
    public Sprite backSprite;

    private SpriteRenderer spriteRenderer;
    private bool isFlipped = false;
    private bool isMatched = false;
    private bool isAnimating = false;
    private Vector3 originalScale;

    public static event Action<Card> OnCardFlipped;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale; // Store prefab’s scale
    }

    void OnMouseDown()
    {
        if (isMatched || isFlipped || isAnimating) return;

        FlipToFront();
        OnCardFlipped?.Invoke(this);
    }

    public void FlipToFront()
    {
        if (isAnimating) return;
        StartCoroutine(FlipAnimation(frontSprite, true));
    }

    public void ShowBack()
    {
        if (isAnimating) return;
        StartCoroutine(FlipAnimation(backSprite, false));
    }

    public void SetMatched()
    {
        isMatched = true;
        StartCoroutine(MatchPopAnimation());
    }
    private IEnumerator MatchPopAnimation()
    {
        yield return new WaitForSecondsRealtime(0.5f); 
        Vector3 maxScale = originalScale * 1.2f;
        Vector3 minScale = originalScale;

        // Grow
        for (float t = 0; t < 0.1f; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(minScale, maxScale, t / 0.1f);
            yield return null;
        }

        // Shrink back
        for (float t = 0; t < 0.1f; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(maxScale, minScale, t / 0.1f);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private IEnumerator FlipAnimation(Sprite newSprite, bool flipped)
    {
        isAnimating = true;

        // Shrink X to 0
        for (float t = 0; t < 0.15f; t += Time.deltaTime)
        {
            float scale = Mathf.Lerp(originalScale.x, 0f, t / 0.15f);
            transform.localScale = new Vector3(scale, originalScale.y, originalScale.z);
            yield return null;
        }

        // Change sprite
        spriteRenderer.sprite = newSprite;
        isFlipped = flipped;

        // Expand X back to original scale
        for (float t = 0; t < 0.15f; t += Time.deltaTime)
        {
            float scale = Mathf.Lerp(0f, originalScale.x, t / 0.15f);
            transform.localScale = new Vector3(scale, originalScale.y, originalScale.z);
            yield return null;
        }

        transform.localScale = originalScale;
        isAnimating = false;
    }
    public void RestoreStateInstant(bool flipped, bool matched)
    {
        // ensure sprites are already assigned before calling this
        isAnimating = false;
        isMatched = matched;
        isFlipped = flipped;

        // show correct face immediately
        if (flipped)
            GetComponent<SpriteRenderer>().sprite = frontSprite;
        else
            GetComponent<SpriteRenderer>().sprite = backSprite;

        // keep original size
        transform.localScale = originalScale;
    }

    public bool IsFlipped => isFlipped;
    public bool IsMatched => isMatched;
}
