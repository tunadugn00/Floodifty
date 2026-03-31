using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance {  get; private set; }

    [SerializeField] private int hintPrice = 100;
    [SerializeField] private int hammerPrice = 200;
    [SerializeField] private int colorBombPrice = 500;

    [SerializeField] private int startingHints = 1;
    [SerializeField] private int startingHammers = 1;
    [SerializeField] private int startingColorBombs = 1;

    private const string HINT_COUNT_KEY = "HintCount";
    private const string HAMMER_COUNT_KEY = "HammerCount";
    private const string BOMB_COUNT_KEY = "ColorBombCount";

    private int hintCount;
    private int hammerCount;
    private int colorBombCount;

    private bool isTutorialMode = false;
    private int backupHints;
    private int backupHammers;
    private int backupBombs;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadInventory();
    }

    private void LoadInventory()
    {
        if (!PlayerPrefs.HasKey(HINT_COUNT_KEY))
        {
            hintCount = startingHints;
            hammerCount = startingHammers;
            colorBombCount = startingColorBombs;
            SaveInventory();
        }
        else
        {
            hintCount = PlayerPrefs.GetInt(HINT_COUNT_KEY, 0);
            hammerCount = PlayerPrefs.GetInt(HAMMER_COUNT_KEY, 0);
            colorBombCount = PlayerPrefs.GetInt(BOMB_COUNT_KEY, 0);
        }
    }
    private void SaveInventory()
    {
        if (isTutorialMode) return;

        PlayerPrefs.SetInt(HINT_COUNT_KEY, hintCount);
        PlayerPrefs.SetInt(HAMMER_COUNT_KEY, hammerCount);
        PlayerPrefs.SetInt(BOMB_COUNT_KEY, colorBombCount);
        PlayerPrefs.Save();
    }
    public bool UseHint()
    {
        if (hintCount > 0)
        {
            hintCount--;
            SaveInventory();
            return true;
        }     
        return false;
    }
    public bool BuyHint()
    {
        if (CurrencyManager.Instance.SpendCoins(hintPrice))
        {
            hintCount++;
            SaveInventory();
            return true;
        }
        return false;
    }

    public void GrantHintFromRewardedAd()
    {
        hintCount++;
        SaveInventory();
    }
    public void GrantHammerFromRewardedAd()
    {
        hammerCount++;
        SaveInventory();
    }
    public int GetHintCount() => hintCount;
    public bool HasHint() => hintCount > 0;
    public int GetHintPrice() => hintPrice;

    // ===== HAMMER =====
    public bool UseHammer()
    {
        if (hammerCount > 0)
        {
            hammerCount--;
            SaveInventory();
            return true;
        }
        return false;
    }

    public bool BuyHammer()
    {
        if (CurrencyManager.Instance.SpendCoins(hammerPrice))
        {
            hammerCount++;
            SaveInventory();
            return true;
        }
        return false;
    }

    public int GetHammerCount() => hammerCount;
    public bool HasHammer() => hammerCount > 0;
    public int GetHammerPrice() => hammerPrice;

    // ===== COLOR BOMB =====

    public bool UseColorBomb()
    {
        if (colorBombCount > 0)
        {
            colorBombCount--;
            SaveInventory();
            return true;
        }
        return false;
    }

    public bool BuyColorBomb()
    {
        if (CurrencyManager.Instance.SpendCoins(colorBombPrice))
        {
            colorBombCount++;
            SaveInventory();
            return true;
        }
        return false;
    }

    public void GrantColorBombFromRewardedAd()
    {
        colorBombCount++;
        SaveInventory();
    }

    public int GetColorBombCount() => colorBombCount;
    public bool HasColorBomb() => colorBombCount > 0;
    public int GetColorBombPrice() => colorBombPrice;

    public void StartTutorialMode()
    {
        isTutorialMode = true;
        backupHints = hintCount;
        backupHammers = hammerCount;
        backupBombs = colorBombCount;

        hintCount = 1;
        hammerCount = 1;
        colorBombCount = 1;
    }

    public void EndTutorialMode()
    {
        isTutorialMode = false;

        hintCount = backupHints;
        hammerCount = backupHammers;
        colorBombCount = backupBombs;

        SaveInventory(); 
    }

}
