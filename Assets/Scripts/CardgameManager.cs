using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

#region Memento Pattern (Save Data)
[System.Serializable]
public class GameSaveData
{
    public int rows;
    public int columns;
    public int score;
    public int combo;
    public int moves;
    public int lives;
    public int turnNumber;
    public int matchNumber;
    public List<int> gridCardTypes;
    public List<bool> gridCardMatched;
}
#endregion

public class CardgameManager : MonoBehaviour
{
    [Header("Singleton")]
    public static CardgameManager Instance;

    [Header("Dependencies")]
    public List<CardInteractable> cardPrefabs;
    public List<CardInteractable> cards;
    
    [Header("Grid Settings")]
    [Range(2, 6)] public int rows = 2;
    [Range(2, 6)] public int columns = 2;
    public float spacing = 1.2f;

    [Header("Camera Settings")]
    public Camera mainCamera;
    public float cameraPadding = 1.0f;

    [Header("Gameplay & Audio")]
    public AudioSource audioSource;
    public AudioClip flipSound;
    public AudioClip matchSound;
    public AudioClip mismatchSound;
    public AudioClip gameOverSound;
    public AudioClip comboSound;

    // Public state for inspector/external viewing
    [Header("Game State")]
    public int score = 0;
    public int combo = 0;
    public int moves = 0;
    public int lives = 3;
    public int level = 1;
    public int matchNumber = 0;
    public int turnNumber = 1;

    private List<CardInteractable> currentlyFlipped = new List<CardInteractable>();
    private int matchedPairsCount = 0;
    private int totalPairs;

    // Services via Facade/Composition
    private GridFactory gridFactory;
    private SaveSystem saveSystem;
    private AudioController audioController;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Initialize Pattern Components
        gridFactory = new GridFactory(this);
        saveSystem = new SaveSystem(this);
        audioController = new AudioController(this);
    }

    void Start()
    {
        level = PlayerPrefs.GetInt("PlayerLevel", 1);
        if (mainCamera == null) mainCamera = Camera.main;
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    // --- FACADE METHODS (Used by UI) ---

    public void LoadGameOrInitialize()
    {
        if (!saveSystem.TryLoadGame())
        {
            InitializeCardSetup();
        }
    }

    public void SaveGame()
    {
        saveSystem.SaveGame();
    }

    public void ClearSave()
    {
        saveSystem.ClearSave();
    }

    public void NextLevel()
    {
        GenerateGrid(rows, columns);
    }

    // --- CORE GAME LOOP & STATE MANAGEMENT ---

    private void InitializeCardSetup()
    {
        ResetGameState();
        UpdateGridSizeForLevel(level);
        UpdateAllUIEvents();
        GenerateGrid(rows, columns);
    }

    private void ResetGameState()
    {
        score = 0;
        combo = 0;
        moves = 0;
        turnNumber = 1;
        matchNumber = 0;
        lives = 3; 
    }

    public void UpdateAllUIEvents()
    {
        GameEvents.OnScoreUpdate?.Invoke(score);
        GameEvents.OnMovesUpdate?.Invoke(moves);
        GameEvents.OnLivesUpdate?.Invoke(lives);
        GameEvents.OnLevelUpdate?.Invoke(level);
        GameEvents.OnMatchNumberUpdated?.Invoke(matchNumber);
        GameEvents.OnTurnNumberUpdated?.Invoke(turnNumber);
    }

    // --- GRID OPERATIONS DELEGATED TO FACTORY ---

    public void GenerateGrid(int r, int c)
    {
        gridFactory.GenerateGrid(r, c);
        currentlyFlipped.Clear();
        SaveGame();
    }

    public void RestoreGrid(GameSaveData data)
    {
        gridFactory.RestoreGrid(data);
        UpdateAllUIEvents();
    }

    private void UpdateGridSizeForLevel(int currentLevel)
    {
        Vector2Int size = gridFactory.GetGridSizeForLevel(currentLevel);
        rows = size.x;
        columns = size.y;
    }

    // --- GAMEPLAY CONTROLLER LOGIC ---

    public void CardFlipped(CardInteractable card)
    {
        audioController.PlaySound(flipSound);
        currentlyFlipped.Add(card);

        if (currentlyFlipped.Count >= 2)
        {
            CardInteractable c1 = currentlyFlipped[0];
            CardInteractable c2 = currentlyFlipped[1];
            currentlyFlipped.Clear(); // Clears both instantly without needing RemoveAt twice

            DOVirtual.DelayedCall(0.5f, () => CompareCards(c1, c2));
        }
    }

    private void CompareCards(CardInteractable c1, CardInteractable c2)
    {
        moves++;
        turnNumber++;
        GameEvents.OnMovesUpdate?.Invoke(moves);
        GameEvents.OnTurnNumberUpdated?.Invoke(turnNumber);

        if (c1.cardType == c2.cardType)
            HandleMatch(c1, c2);
        else
            HandleMismatch(c1, c2);

        if (matchedPairsCount < totalPairs)
        {
            SaveGame();
        }
    }

    private void HandleMatch(CardInteractable c1, CardInteractable c2)
    {
        combo++;
        score += 10 * combo;
        matchNumber++;
        matchedPairsCount++;

        GameEvents.OnScoreUpdate?.Invoke(score);
        GameEvents.OnMatchNumberUpdated?.Invoke(matchNumber);

        if (combo > 1)
        {
            GameEvents.OnComboUpdate?.Invoke(combo);
            audioController.PlaySound(comboSound);
        }
        else
        {
            audioController.PlaySound(matchSound);
        }

        c1.Match();
        c2.Match();

        CheckWinCondition();
    }

    private void HandleMismatch(CardInteractable c1, CardInteractable c2)
    {
        audioController.PlaySound(mismatchSound);
        combo = 0;
        score = Mathf.Max(0, score - 2);
        lives--;

        GameEvents.OnScoreUpdate?.Invoke(score);
        GameEvents.OnLivesUpdate?.Invoke(lives);

        if (mainCamera != null)
        {
            mainCamera.transform.DOComplete();
            mainCamera.transform.DOShakePosition(0.1f, 0.2f, 10, 90f, false, true);
        }

        c1.CloseCardWait();
        c2.CloseCardWait();
    }

    private void CheckWinCondition()
    {
        if (matchedPairsCount >= totalPairs)
        {
            ClearSave();
            Debug.Log($"Level {level} Complete! Score: {score}, Final Combo: {combo}");
            UpdateDataForNextLevel(); 
            
            DOVirtual.DelayedCall(1.5f, () => { 
                audioController.PlaySound(gameOverSound);
                GameEvents.OnGameWin?.Invoke();
            });
        }
    }

    public void UpdateDataForNextLevel()
    {
        level++;
        PlayerPrefs.SetInt("PlayerLevel", level);
        PlayerPrefs.Save();
        
        UpdateGridSizeForLevel(level);
        
        moves = 0;
        turnNumber = 1;
        combo = 0;
        matchNumber = 0;

        UpdateAllUIEvents();
    }

    // Allow components to sync pair tracking
    public void SetGridPairs(int pairs, int matchedCount)
    {
        totalPairs = pairs;
        this.matchedPairsCount = matchedCount;
        matchNumber = matchedCount;
    }
}

#region Design Patterns implementation (Factory, Save/Memento, Audio Controller)

public class GridFactory
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

        List<CardInteractable> cardsToInstantiate = new List<CardInteractable>();

        for (int i = 0; i < totalPairs; i++)
        {
            if (ctx.cardPrefabs == null || ctx.cardPrefabs.Count == 0) break;
            CardInteractable prefab = ctx.cardPrefabs[i % ctx.cardPrefabs.Count];
            cardsToInstantiate.Add(prefab);
            cardsToInstantiate.Add(prefab);
        }

        if (cardsToInstantiate.Count < totalExpectedCards && ctx.cardPrefabs != null && ctx.cardPrefabs.Count > 0)
            cardsToInstantiate.Add(ctx.cardPrefabs[0]);

        // Shuffle
        for (int i = 0; i < cardsToInstantiate.Count; i++)
        {
            CardInteractable temp = cardsToInstantiate[i];
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
                CardInteractable newCard = Object.Instantiate(cardsToInstantiate[i], cardPos, Quaternion.identity);
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

            CardInteractable prefab = null;
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
                CardInteractable newCard = Object.Instantiate(prefab, cardPos, Quaternion.identity);
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
        if (ctx.cards == null) ctx.cards = new List<CardInteractable>();
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

public class SaveSystem
{
    private CardgameManager ctx;
    private const string SaveKey = "CardGameSaveData";

    public SaveSystem(CardgameManager context)
    {
        ctx = context;
    }

    public bool TryLoadGame()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            
            if (data != null && data.gridCardTypes.Count == data.rows * data.columns)
            {
                ctx.score = data.score;
                ctx.combo = data.combo;
                ctx.moves = data.moves;
                ctx.lives = data.lives <= 0 ? 3 : data.lives; // Restores lives correctly 
                ctx.turnNumber = data.turnNumber;
                
                ctx.RestoreGrid(data);
                return true;
            }
        }
        return false;
    }

    public void SaveGame()
    {
        GameSaveData data = new GameSaveData
        {
            rows = ctx.rows,
            columns = ctx.columns,
            score = ctx.score,
            combo = ctx.combo,
            moves = ctx.moves,
            lives = ctx.lives,
            turnNumber = ctx.turnNumber,
            matchNumber = ctx.matchNumber,
            gridCardTypes = new List<int>(),
            gridCardMatched = new List<bool>()
        };

        foreach (var c in ctx.cards)
        {
            data.gridCardTypes.Add((int)c.cardType);
            data.gridCardMatched.Add(c.cardState == CardState.Matched);
        }

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
    }
}

public class AudioController
{
    private CardgameManager ctx;

    public AudioController(CardgameManager context)
    {
        ctx = context;
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && ctx.audioSource != null)
            ctx.audioSource.PlayOneShot(clip);
    }
}
#endregion
