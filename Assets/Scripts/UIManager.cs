using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI matchNumberText;
    public TextMeshProUGUI turnNumberText;
    public TextMeshProUGUI movesText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        levelText.text = GameConstants.LevelPrefix + PlayerPrefs.GetInt("PlayerLevel", 1);
    }

    private void OnEnable()
    {
        GameEvents.OnScoreUpdate += UpdateScore;
        GameEvents.OnLevelUpdate += UpdateLevel;
        GameEvents.OnTurnNumberUpdated += UpdateTurnNumber;
        GameEvents.OnMovesUpdate += UpdateMoves;
    }

    private void OnDisable()
    {
        GameEvents.OnScoreUpdate -= UpdateScore;
        GameEvents.OnLevelUpdate -= UpdateLevel;
        GameEvents.OnTurnNumberUpdated -= UpdateTurnNumber;
        GameEvents.OnMovesUpdate -= UpdateMoves;
    }

    private void UpdateScore(int score) 
    {
        scoreText.text = GameConstants.ScorePrefix + score;
    }

    private void UpdateLevel(int level)
    {
        levelText.text = GameConstants.LevelPrefix + level;
    }

    private void UpdateMatchNumber(int matchNumber)
    {
        matchNumberText.text = GameConstants.MatchPrefix + matchNumber;
    }

    private void UpdateTurnNumber(int turnNumber)
    {
        turnNumberText.text = GameConstants.TurnPrefix + turnNumber;
    }

    private void UpdateMoves(int moves)
    {
        movesText.text = GameConstants.MovesPrefix + moves;
    }

    public void OnPlayButtonClicked()
    {
        CardgameManager.Instance.LoadGameOrInitialize();
    }
}
