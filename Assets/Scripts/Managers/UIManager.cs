using UnityEngine;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD Text Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI matchNumberText;
    public TextMeshProUGUI turnNumberText;
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI gameLevelText;

    [Header("Panels & Effects")]
    public GameObject comboEffect;
    public GameObject winPanel;

    // Services via Composition
    private HUDController hudController;
    private PanelController panelController;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Initialize pattern components
        hudController = new HUDController(this);
        panelController = new PanelController(this);
    }

    private void Start()
    {
        hudController.Initialize();
    }

    private void OnEnable()
    {
        GameEvents.OnScoreUpdate += hudController.UpdateScore;
        GameEvents.OnLevelUpdate += hudController.UpdateLevel;
        GameEvents.OnTurnNumberUpdated += hudController.UpdateTurnNumber;
        GameEvents.OnMatchNumberUpdated += hudController.UpdateMatchNumber;
        GameEvents.OnMovesUpdate += hudController.UpdateMoves;
        GameEvents.OnComboUpdate += panelController.ShowCombo;
        GameEvents.OnGameWin += panelController.ShowWinPanel;
    }

    private void OnDisable()
    {
        GameEvents.OnScoreUpdate -= hudController.UpdateScore;
        GameEvents.OnLevelUpdate -= hudController.UpdateLevel;
        GameEvents.OnTurnNumberUpdated -= hudController.UpdateTurnNumber;
        GameEvents.OnMatchNumberUpdated -= hudController.UpdateMatchNumber;
        GameEvents.OnMovesUpdate -= hudController.UpdateMoves;
        GameEvents.OnComboUpdate -= panelController.ShowCombo;
        GameEvents.OnGameWin -= panelController.ShowWinPanel;       
    }

    // --- Input Handlers (Facade) ---
    public void OnPlayButtonClicked()
    {
        CardgameManager.Instance.LoadGameOrInitialize();
    }

    public void OnClickNextLevel()
    {
        panelController.HideWinPanel();
        CardgameManager.Instance.NextLevel();
    }
}

#region Design Patterns Implementation (Controllers)

public class HUDController
{
    private UIManager ctx;

    public HUDController(UIManager context)
    {
        ctx = context;
    }

    public void Initialize()
    {
        int level = PlayerPrefs.GetInt("PlayerLevel", 1);
        UpdateLevel(level);
    }

    public void UpdateScore(int score) 
    {
        if (ctx.scoreText != null) ctx.scoreText.text = GameConstants.ScorePrefix + score;
    }

    public void UpdateLevel(int level)
    {
        if (ctx.levelText != null) ctx.levelText.text = GameConstants.LevelPrefix + level;
        if (ctx.gameLevelText != null) ctx.gameLevelText.text = GameConstants.LevelPrefix + level;
    }

    public void UpdateMatchNumber(int matchNumber)
    {
        if (ctx.matchNumberText != null) ctx.matchNumberText.text = GameConstants.MatchPrefix + matchNumber;
    }

    public void UpdateTurnNumber(int turnNumber)
    {
        if (ctx.turnNumberText != null) ctx.turnNumberText.text = GameConstants.TurnPrefix + turnNumber;
    }

    public void UpdateMoves(int moves)
    {
        if (ctx.movesText != null) ctx.movesText.text = GameConstants.MovesPrefix + moves;
    }
}

public class PanelController
{
    private UIManager ctx;

    public PanelController(UIManager context)
    {
        ctx = context;
    }

    public void ShowCombo(int combo)
    {
        if (ctx.comboEffect == null || ctx.comboText == null) return;

        ctx.comboEffect.SetActive(combo > 1);
        
        if (combo > 1)
        {
            ctx.comboEffect.transform.localScale = Vector3.zero;
            ctx.comboEffect.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            ctx.comboText.text = "Combo X" + combo;
            
            DOVirtual.DelayedCall(0.75f, () => {
                if (ctx != null && ctx.comboEffect != null)
                    ctx.comboEffect.SetActive(false);
            });
        }
    }

    public void ShowWinPanel()
    {
        if (ctx.winPanel != null)
        {
            ctx.winPanel.SetActive(true);
            ctx.winPanel.transform.localScale = Vector3.zero;
            ctx.winPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
    }

    public void HideWinPanel()
    {
        if (ctx.winPanel != null)
        {
            ctx.winPanel.SetActive(false);
        }
    }
}
#endregion
