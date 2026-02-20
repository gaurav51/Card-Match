using DG.Tweening;
using UnityEngine;

public class CardController : MonoBehaviour
{
    [Header("State")]
    public CardState cardState;
    public CardType cardType;
    public bool canBeFlipped = true;

    [Header("Visual Config")]
    [SerializeField] private Vector3 childPos = new Vector3(0, 0, -0.01f);
    [SerializeField] private Vector3 childScale = new Vector3(0.7f, 1, 1);
    
    [Header("References")]
    [SerializeField] private Transform cardFront;
    [SerializeField] private Transform cardIcon;

    private void Start()
    {
        // Set visual scales to properly format the cards
        if (transform.childCount >= 3)
        {
            if (cardFront == null) cardFront = transform.GetChild(0);
            if (cardIcon == null) cardIcon = transform.GetChild(2);

            cardFront.localPosition = childPos;
            cardIcon.localScale = childScale;
        }
    }

    private void OnEnable()
    {
        cardState = CardState.Hidden;
    }

    private void OnDisable()
    {
        cardState = CardState.Hidden;
    }

    #region Card Actions

    public void Match()
    {
        cardState = CardState.Matched;
        canBeFlipped = false;
        transform.DOShakeScale(0.3f, 0.2f);
    }

    public void ForceMatch()
    {
        cardState = CardState.Matched;
        canBeFlipped = false;
        transform.eulerAngles = new Vector3(0, 180, 0); // Open immediately
        transform.localScale = Vector3.one;
    }

    public void CloseCardWait()
    {
        transform.DORotate(Vector3.zero, 0.1f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                cardState = CardState.Hidden;
                canBeFlipped = true;
            });
    }

    private void PerformFlip()
    {
        canBeFlipped = false;
        transform.DORotate(new Vector3(0, 180, 0), 0.5f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                cardState = CardState.Revealed;
                CardgameManager.Instance.CardFlipped(this);
            });
    }

    #endregion

    #region Input

    private void OnMouseDown()
    {
        if (canBeFlipped && cardState == CardState.Hidden)
        {
            PerformFlip();
        }
    }

    #endregion

    #region Initialization

    public void SetCardType(CardType type)
    {
        cardType = type;
    }

    public void InitializeCard()
    {
        cardState = CardState.Hidden;
        canBeFlipped = true;
        
        transform.localScale = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0, 180, 0);

        transform.DOScale(Vector3.one, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            });
    }

    #endregion
}
