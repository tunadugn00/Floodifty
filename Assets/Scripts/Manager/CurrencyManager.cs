using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    private int coins = 0;
    private const string COINT_KEY = "PlayerCoins";

    public event Action<int> OnCoinsChanged;

    [SerializeField] private int startingCoins = 500;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadCoins();
    }

    private void LoadCoins()
    {
        if (PlayerPrefs.HasKey(COINT_KEY))
        {
            coins = PlayerPrefs.GetInt(COINT_KEY);
        }
        else
        {
            coins = startingCoins;
            SaveCoins();
        }
    }
    private void SaveCoins()
    {
        PlayerPrefs.SetInt(COINT_KEY, coins);
        PlayerPrefs.Save();
    }

    public int GetCoins()
    {
        return coins;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        coins += amount;
        SaveCoins();
        OnCoinsChanged?.Invoke(coins);
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0) return false;
        if (coins < amount) return false;
        coins  -= amount;
        SaveCoins();

        OnCoinsChanged?.Invoke(coins);
        return true;
    }

    public bool HasEnoughCoins(int amount)
    {
        return coins >= amount;
    }

    public void SetCoins(int amount)
    {
        coins = Mathf.Max(0, amount);
        SaveCoins();
        OnCoinsChanged?.Invoke(coins);
        Debug.Log($"[Currency] Set coins to: {coins}");
    }

    // ===== DEBUG =====
    [ContextMenu("Add 1000 Coins (Cheat)")]
    private void CheatAddCoins()
    {
        AddCoins(1000);
    }
    [ContextMenu("Del 1000 Coins (Cheat)")]
    private void CheatDelCoins()
    {
        AddCoins(-1000);
    }

    [ContextMenu("Reset Coins")]
    private void ResetCoins()
    {
        SetCoins(startingCoins);
    }

    [ContextMenu("Print Coins")]
    private void PrintCoins()
    {
        Debug.Log($"Current coins: {coins}");
    }
}
