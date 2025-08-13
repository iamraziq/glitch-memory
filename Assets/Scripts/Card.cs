using UnityEngine;
using System;

public class Card : MonoBehaviour
{
    public int cardId;
    public Sprite frontSprite;
    public Sprite backSprite;

    private SpriteRenderer spriteRenderer;
    private bool isFlipped = false;
    private bool isMatched = false;

    public static event Action<Card> OnCardFlipped;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Don't call ShowBack() here — wait until sprites are assigned
    }

    void OnMouseDown()
    {
        if (isMatched || isFlipped) return;

        Flip();
        OnCardFlipped?.Invoke(this);
    }

    public void Flip()
    {
        isFlipped = true;
        spriteRenderer.sprite = frontSprite;
    }

    public void ShowBack()
    {
        isFlipped = false;
        spriteRenderer.sprite = backSprite;
    }

    public void SetMatched()
    {
        isMatched = true;
    }

    public bool IsFlipped => isFlipped;
    public bool IsMatched => isMatched;
}
