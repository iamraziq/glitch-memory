using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Sprite[] cardFrontSprites;
    public Sprite cardBackSprite;

    public int rows = 2;
    public int cols = 2;
    public float spacing = 0.2f;

    private List<Card> flippedCards = new List<Card>();
    public ScoreManager scoreManager;
    public AudioManager audioManager;
    void OnEnable() => Card.OnCardFlipped += HandleCardFlipped;
    void OnDisable() => Card.OnCardFlipped -= HandleCardFlipped;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // Create list of IDs (pairs)
        List<int> ids = new List<int>();
        int totalCards = rows * cols;
        for (int i = 0; i < totalCards / 2; i++)
        {
            ids.Add(i);
            ids.Add(i);
        }

        // Shuffle
        for (int i = 0; i < ids.Count; i++)
        {
            int rand = Random.Range(i, ids.Count);
            (ids[i], ids[rand]) = (ids[rand], ids[i]);
        }

        // Instantiate
        float startX = -(cols - 1) / 2f;
        float startY = -(rows - 1) / 2f;

        int index = 0;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                GameObject obj = Instantiate(cardPrefab, transform);
                obj.transform.localPosition = new Vector3(
                    (x + startX) + x * spacing,
                    (y + startY) + y * spacing,
                    0
                );
                Card card = obj.GetComponent<Card>();
                int id = ids[index];
                card.cardId = id;
                card.frontSprite = cardFrontSprites[id % cardFrontSprites.Length];
                card.backSprite = cardBackSprite;
                card.ShowBack(); // ✅ Now ensures back is shown after assigning
                index++;
            }
        }
    }

    //void HandleCardFlipped(Card card)
    //{
    //    flippedCards.Add(card);

    //    if (flippedCards.Count == 2)
    //    {
    //        CheckMatch();
    //    }
    //}
    void HandleCardFlipped(Card card)
    {
        audioManager?.PlayFlip();
        flippedCards.Add(card);

        if (flippedCards.Count == 2)
        {
            CheckMatch();
        }
    }

    //void CheckMatch()
    //{
    //    Card card1 = flippedCards[0];
    //    Card card2 = flippedCards[1];

    //    if (card1.cardId == card2.cardId)
    //    {
    //        card1.SetMatched();
    //        card2.SetMatched();

    //    }
    //    else
    //    {
    //        StartCoroutine(FlipBackAfterDelay(card1, card2));
    //    }

    //    flippedCards.Clear();
    //}
    void CheckMatch()
    {
        Card card1 = flippedCards[0];
        Card card2 = flippedCards[1];

        if (card1.cardId == card2.cardId)
        {
            card1.SetMatched();
            card2.SetMatched();
            scoreManager?.AddScore(10);
            audioManager?.PlayMatch();

            if (AllCardsMatched())
            {
                audioManager?.PlayGameOver();
            }
        }
        else
        {
            scoreManager?.SubtractScore(2);
            audioManager?.PlayMismatch();
            StartCoroutine(FlipBackAfterDelay(card1, card2));
        }

        flippedCards.Clear();
    }
    bool AllCardsMatched()
    {
        Card[] allCards = FindObjectsOfType<Card>();
        foreach (var card in allCards)
        {
            if (!card.IsMatched) return false;
        }
        return true;
    }

    IEnumerator FlipBackAfterDelay(Card c1, Card c2)
    {
        yield return new WaitForSeconds(1f);
        if (!c1.IsMatched) c1.ShowBack();
        if (!c2.IsMatched) c2.ShowBack();
    }

}
