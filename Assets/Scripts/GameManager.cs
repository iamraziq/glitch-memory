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
    private bool isLoading = false;
    [SerializeField] private float initialRevealTime = 2f; 
    private bool isRevealing = false;
    void OnEnable() => Card.OnCardFlipped += HandleCardFlipped;
    void OnDisable() => Card.OnCardFlipped -= HandleCardFlipped;

    void Start()
    {
        if (!LoadGame())
        {
            StartCoroutine(StartWithReveal());
        }
    }
    IEnumerator StartWithReveal()
    {
        isRevealing = true;
        GenerateGrid();

        // Show all cards face-up instantly
        Card[] cards = FindObjectsOfType<Card>();
        foreach (var card in cards)
        {
            card.RestoreStateInstant(true, false);
        }

        // Wait for the reveal time
        yield return new WaitForSeconds(initialRevealTime);

        // Flip all cards back
        foreach (var card in cards)
        {
            if (!card.IsMatched)
                card.ShowBack();
        }

        isRevealing = false;
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
                if (!isLoading && !isRevealing)
                    card.ShowBack();
                index++;
            }
        }
    }

    void HandleCardFlipped(Card card)
    {
        audioManager?.PlayFlip();
        flippedCards.Add(card);

        if (flippedCards.Count == 2)
        {
            CheckMatch();
        }

        SaveGame();
    }

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
                SaveLoadManager.ClearSave(); // clear progress after completion
            }
        }
        else
        {
            scoreManager?.SubtractScore(2);
            audioManager?.PlayMismatch();
            StartCoroutine(FlipBackAfterDelay(card1, card2));
        }

        flippedCards.Clear();
        SaveGame();
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
        SaveGame();
    }

    // ---------------- SAVE / LOAD ----------------
    void SaveGame()
    {
        SaveData data = new SaveData();
        data.score = scoreManager.CurrentScore;
        data.gridRows = rows;
        data.gridCols = cols;

        Card[] allCards = FindObjectsOfType<Card>();
        data.cards = new CardData[allCards.Length];

        for (int i = 0; i < allCards.Length; i++)
        {
            bool saveAsFlipped = allCards[i].IsMatched;
            // Only matched cards will be stored as flipped

            data.cards[i] = new CardData
            {
                cardId = allCards[i].cardId,
                isMatched = allCards[i].IsMatched,
                isFlipped = saveAsFlipped
            };
        }

        SaveLoadManager.SaveGame(data);
    }


    bool LoadGame()
    {
        SaveData data = SaveLoadManager.LoadGame();
        if (data == null) return false;

        isLoading = true; // prevent ShowBack during grid creation

        rows = data.gridRows;
        cols = data.gridCols;

        // Rebuild grid
        GenerateGrid();

        // Restore score
        scoreManager.AddScore(data.score - scoreManager.CurrentScore);

        // Restore card states
        Card[] allCards = FindObjectsOfType<Card>();
        for (int i = 0; i < allCards.Length && i < data.cards.Length; i++)
        {
            var c = allCards[i];
            var saved = data.cards[i];

            c.cardId = saved.cardId;
            c.frontSprite = cardFrontSprites[saved.cardId % cardFrontSprites.Length];
            c.backSprite = cardBackSprite;

            bool shouldBeFaceUp = saved.isMatched || saved.isFlipped;
            c.RestoreStateInstant(shouldBeFaceUp, saved.isMatched);
        }

        isLoading = false; // done loading
        return true;
    }

}
