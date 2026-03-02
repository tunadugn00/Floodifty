using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CoinDisplay : MonoBehaviour
{
    private TextMeshProUGUI coinText;

    [Header("Counter Animation")]
    [SerializeField] private float counterDuration = 0.5f;
    [SerializeField] private Ease counterEase = Ease.OutQuad;

    [Header("Scale Punch")]
    [SerializeField] private bool usePunchScale = true;
    [SerializeField] private float punchScale = 0.3f;
    [SerializeField] private float punchDuration = 0.3f;

    private int currentValue = 0;

    void Awake()
    {
        coinText = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged += UpdateDisplay;
            currentValue = CurrencyManager.Instance.GetCoins();
            coinText.text = currentValue.ToString("N0");
        }
    }

    void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged -= UpdateDisplay;
        }
    }

    private void UpdateDisplay(int newAmount)
    {
        DOTween.Kill(coinText);
        DOTween.Kill(transform);

        SoundManager.Instance?.PlayCoin();

        DOTween.To(
            () => currentValue,          
            x => {
                currentValue = x;        
                coinText.text = x.ToString("N0"); 
            },
            newAmount,                  
            counterDuration               
        ).SetEase(counterEase);

        if (usePunchScale)
        {
            transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 5, 0.5f);
        }
    }
}