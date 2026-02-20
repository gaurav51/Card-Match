using System.Collections.Generic;
using UnityEngine;

public class GridFactory : IGridService
{
    private CardgameManager ctx;

    public GridFactory(CardgameManager context)
    {
        ctx = context;
    }

    public void GenerateGrid(int r, int c)
    {
        float startX = -(c - 1) * ctx.spacing / 2f;
        float startY = (r - 1) * ctx.spacing / 2f;

        int totalExpectedCards = r * c;
        int totalPairs = totalExpectedCards / 2;
        ctx.SetGridPairs(totalPairs, 0);

        List<CardController> cardsToInstantiate = new List<CardController>();

        for (int i = 0; i < totalPairs; i++)
        {
            if (ctx.cardPrefabs == null || ctx.cardPrefabs.Count == 0) break;
            CardController prefab = ctx.cardPrefabs[i % ctx.cardPrefabs.Count];
            cardsToInstantiate.Add(prefab);
            cardsToInstantiate.Add(prefab);
        }

        if (cardsToInstantiate.Count < totalExpectedCards && ctx.cardPrefabs != null && ctx.cardPrefabs.Count > 0)
            cardsToInstantiate.Add(ctx.cardPrefabs[0]);

        // Shuffle
        for (int i = 0; i < cardsToInstantiate.Count; i++)
        {
            CardController temp = cardsToInstantiate[i];
            int randomIndex = Random.Range(i, cardsToInstantiate.Count);
            cardsToInstantiate[i] = cardsToInstantiate[randomIndex];
            cardsToInstantiate[randomIndex] = temp;
        }

        ClearExistingCards();

        for (int i = 0; i < totalExpectedCards; i++)
        {
            int row = i / c;
            int col = i % c;

            Vector3 cardPos = new Vector3(startX + col * ctx.spacing, startY - row * ctx.spacing, 0f);

            if (i < cardsToInstantiate.Count)
            {
                CardController newCard = Object.Instantiate(cardsToInstantiate[i], cardPos, Quaternion.identity);
                newCard.transform.parent = ctx.transform;
                newCard.InitializeCard();
                ctx.cards.Add(newCard);
            }
        }

        AdjustCameraSize(r, c);
    }

    public void RestoreGrid(GameSaveData data)
    {
        float startX = -(data.columns - 1) * ctx.spacing / 2f;
        float startY = (data.rows - 1) * ctx.spacing / 2f;

        ctx.rows = data.rows;
        ctx.columns = data.columns;

        int totalExpectedCards = data.rows * data.columns;
        int totalPairs = totalExpectedCards / 2;
        int restoredMatchedCount = 0;

        ClearExistingCards();

        for (int i = 0; i < totalExpectedCards; i++)
        {
            int row = i / data.columns;
            int col = i % data.columns;

            Vector3 cardPos = new Vector3(startX + col * ctx.spacing, startY - row * ctx.spacing, 0f);

            CardController prefab = null;
            if (ctx.cardPrefabs != null && ctx.cardPrefabs.Count > 0)
            {
                foreach (var p in ctx.cardPrefabs)
                {
                    if ((int)p.cardType == data.gridCardTypes[i])
                    {
                        prefab = p;
                        break;
                    }
                }
                if (prefab == null) prefab = ctx.cardPrefabs[0];
            }

            if (prefab != null)
            {
                CardController newCard = Object.Instantiate(prefab, cardPos, Quaternion.identity);
                newCard.transform.parent = ctx.transform;
                newCard.cardType = (CardType)data.gridCardTypes[i];

                if (data.gridCardMatched[i])
                {
                    newCard.ForceMatch();
                    restoredMatchedCount++;
                }
                else
                {
                    newCard.InitializeCard();
                }

                ctx.cards.Add(newCard);
            }
        }

        ctx.SetGridPairs(totalPairs, restoredMatchedCount / 2);
        AdjustCameraSize(data.rows, data.columns);
    }

    private void ClearExistingCards()
    {
        if (ctx.cards == null) ctx.cards = new List<CardController>();
        foreach (var card in ctx.cards)
            if (card != null) Object.Destroy(card.gameObject);
        ctx.cards.Clear();
    }

    private void AdjustCameraSize(int r, int c)
    {
        if (ctx.mainCamera == null) return;
        float targetHeight = r * ctx.spacing;
        float targetWidth = c * ctx.spacing;
        float screenAspect = (float)Screen.width / Screen.height;
        ctx.mainCamera.orthographicSize = Mathf.Max(targetHeight / 2f, targetWidth / (2f * screenAspect)) + ctx.cameraPadding;
    }

    public Vector2Int GetGridSizeForLevel(int currentLevel)
    {
        int[,] gridSizes = new int[,] 
        {
            {2, 2}, {2, 2}, {3, 2}, {3, 2}, {4, 2}, {4, 2}, {5, 2}, {5, 2}, 
            {4, 3}, {4, 3}, {7, 2}, {7, 2}, {4, 4}, {4, 4}, {6, 3}, {6, 3}, 
            {5, 4}, {5, 4}, {6, 4}, {6, 4}, {6, 5}, {6, 6}  
        };

        int index = Mathf.Clamp(currentLevel - 1, 0, gridSizes.GetLength(0) - 1);
        return new Vector2Int(gridSizes[index, 0], gridSizes[index, 1]);
    }
}
