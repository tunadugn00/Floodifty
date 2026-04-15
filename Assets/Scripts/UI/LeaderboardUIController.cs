using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LeaderboardUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup leaderboardPanel;
    [SerializeField] private RectTransform leaderboardWindow;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private GameObject myRow;

    [Header("Loading")]
    [SerializeField] private GameObject loadingIndicator; // Text hoặc Spinner

    [SerializeField] private int fetchCount = 50;

    private List<GameObject> spawnedRows = new List<GameObject>();
    private Color highlightColor = new Color(1f, 0.76f, 0f);

    public void OpenLeaderboard()
    {
        leaderboardPanel.gameObject.SetActive(true);
        leaderboardPanel.alpha = 0;
        leaderboardWindow.localScale = Vector3.zero;
        leaderboardPanel.blocksRaycasts = true;
        leaderboardPanel.interactable = true;

        leaderboardPanel.DOFade(1f, 0.3f);
        leaderboardWindow.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

        LoadLeaderboard();
    }

    public void CloseLeaderboard()
    {
        leaderboardPanel.blocksRaycasts = false;
        leaderboardPanel.interactable = false;

        leaderboardPanel.DOFade(0f, 0.2f);
        leaderboardWindow.DOScale(0f, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => leaderboardPanel.gameObject.SetActive(false));
    }

    async void LoadLeaderboard()
    {
        // Clear rows cũ
        foreach (var r in spawnedRows) Destroy(r);
        spawnedRows.Clear();
        myRow.SetActive(false);

        // Hiện loading
        if (loadingIndicator != null) loadingIndicator.SetActive(true);

        if (LeaderboardManager.Instance == null || !LeaderboardManager.Instance.IsInitialized)
        {
            if (loadingIndicator != null) loadingIndicator.SetActive(false);
            return;
        }

        string myPlayerId = AuthenticationService.Instance.PlayerId;

        var entries = await LeaderboardManager.Instance.GetTopScoresAsync(fetchCount);
        var myEntry = await LeaderboardManager.Instance.GetPlayerScoreAsync();

        // Ẩn loading
        if (loadingIndicator != null) loadingIndicator.SetActive(false);

        if (entries == null) return;


        foreach (var entry in entries)
        {
            bool isMe = entry.PlayerId == myPlayerId;
            SpawnRow(entry, isMe);
        }

        if (myEntry != null)
        {
            myRow.SetActive(true);
            FillRow(myRow, myEntry, true, false);
        }
    }

    void SpawnRow(LeaderboardEntry entry, bool isMe)
    {
        GameObject row = Instantiate(rowPrefab, content);
        spawnedRows.Add(row);
        FillRow(row, entry, isMe, true);
    }

    void FillRow(GameObject row, LeaderboardEntry entry, bool isMe, bool overrideBg)
    {
        int rank = entry.Rank + 1;

        Color rankColor = rank switch
        {
            1 => new Color(1f, 0.84f, 0f),
            2 => new Color(0.75f, 0.75f, 0.75f),
            3 => new Color(0.8f, 0.5f, 0.2f),
            _ => Color.white
        };
        Color textColor = isMe ? highlightColor : rankColor;

        var textRank = row.transform.Find("TextRank")?.GetComponent<TextMeshProUGUI>();
        if (textRank != null)
        {
            textRank.text = rank switch { 1 => "1st", 2 => "2nd", 3 => "3rd", _ => rank.ToString() };
            textRank.color = textColor;
        }

        var textName = row.transform.Find("TextName")?.GetComponent<TextMeshProUGUI>();
        if (textName != null)
        {
            string name = entry.PlayerName ?? "Player_????";
            int hash = name.IndexOf('#');
            if (hash > 0) name = name.Substring(0, hash);
            textName.text = name;
            textName.color = textColor;
        }

        var textScore = row.transform.Find("TextScore")?.GetComponent<TextMeshProUGUI>();
        if (textScore != null)
        {
            textScore.text = ((int)entry.Score).ToString("N0");
            textScore.color = textColor;
        }

        var bg = row.transform.Find("Background")?.GetComponent<Image>();
        if (bg != null && overrideBg)
        {
            bg.color = rank switch
            {
                1 => new Color(1f, 0.84f, 0f, 0.15f),
                2 => new Color(0.75f, 0.75f, 0.75f, 0.15f),
                3 => new Color(0.8f, 0.5f, 0.2f, 0.15f),
                _ => isMe ? new Color(1f, 0.76f, 0f, 0.15f) : new Color(1f, 1f, 1f, 0.05f)
            };
        }
    }
}