using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardgameManager : MonoBehaviour
{
    [Header("Singleton")]
    public static CardgameManager Instance;

    [Header("Dependencies")]
    public List<CardController> cardPrefabs;
    public List<CardController> cards;
    
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

    private List<CardController> currentlyFlipped = new List<CardController>();
    private int matchedPairsCount = 0;
    private int totalPairs;

    // Services typed to interfaces (Dependency Inversion Principle)
    private IGridService gridFactory;
    private ISaveService saveSystem;
    private IAudioService audioController;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Initialize Interfaces / Services
        gridFactory = new GridFactory(this);
        saveSystem = new SaveSystem(this);
    }

    void Start()
    {
        level = PlayerPrefs.GetInt("PlayerLevel", 1);
        if (mainCamera == null) mainCamera = Camera.main;
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        // Audio is decoupled, requiring only AudioSource
        audioController = new AudioController(audioSource);
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

    public void CardFlipped(CardController card)
    {
        audioController.PlaySound(flipSound);
        currentlyFlipped.Add(card);

        if (currentlyFlipped.Count >= 2)
        {
            CardController c1 = currentlyFlipped[0];
            CardController c2 = currentlyFlipped[1];
            currentlyFlipped.Clear(); // Clears both instantly without needing RemoveAt twice

            DOVirtual.DelayedCall(0.5f, () => CompareCards(c1, c2));
        }
    }

    private void CompareCards(CardController c1, CardController c2)
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

    private void HandleMatch(CardController c1, CardController c2)
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

    private void HandleMismatch(CardController c1, CardController c2)
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
