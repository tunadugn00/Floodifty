using UnityEngine;
public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("Reward Settings")]
    [SerializeField] private int baseReward = 50;       
    [SerializeField] private int bonusPerStar = 25;     

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GiveReward(int stars)
    {
        int totalReward = baseReward + (stars - 1) * bonusPerStar;
        CurrencyManager.Instance?.AddCoins(totalReward);

        return totalReward;
    }

    public int GetBaseReward() => baseReward;
    public int GetBonusPerStar() => bonusPerStar;
}